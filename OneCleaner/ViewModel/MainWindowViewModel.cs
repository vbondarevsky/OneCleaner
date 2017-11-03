using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace OneCleaner
{
    public class MainWindowViewModel : BaseViewModel
    {
        public Status Status { get; private set; }
        public double Progress { get; private set; }

        public bool InstalledVersionsArePopulating { get; private set; }
        public bool CacheArePopulating { get; private set; }
        public bool InfoBasesArePopulating { get; private set; }

        public ObservableCollection<InstalledVersionItemViewModel> InstalledVersions { get; private set; }
        public ObservableCollection<CacheItemViewModel> Cache { get; private set; }
        public ObservableCollection<InfoBaseItemViewModel> InfoBases { get; private set; }

        public ICommand UninstallCommand { get; set; }
        public ICommand SelectAllCommand { get; set; }
        public ICommand UnselectAllCommand { get; set; }

        public ICommand InstalledVersionsSortCommand { get; set; }
        public ICommand InfoBasesSortCommand { get; set; }
        public ICommand CacheSortCommand { get; set; }

        public ICommand RemoveCacheCommand { get; private set; }

        public ICommand RemoveInfoBaseCommand { get; private set; }

        public MainWindowViewModel()
        {
            Status = Status.Idle;

            InstalledVersions = new ObservableCollection<InstalledVersionItemViewModel>();
            PopulateInstalledVersions();

            InfoBases = new ObservableCollection<InfoBaseItemViewModel>();
            Cache = new ObservableCollection<CacheItemViewModel>();
            PopulateInfoBasesAndCache();

            InstalledVersionsSortCommand = new RelayCommand(p => { Sort(CollectionViewSource.GetDefaultView(InstalledVersions), (string)p); });
            InfoBasesSortCommand = new RelayCommand(p => { Sort(CollectionViewSource.GetDefaultView(InfoBases), (string)p); });
            CacheSortCommand = new RelayCommand(p => { Sort(CollectionViewSource.GetDefaultView(Cache), (string)p); });

            UninstallCommand = new RelayCommand(p => { Uninstall(); });

            SelectAllCommand = new RelayCommand(
                p =>
                {
                    (p as IEnumerable<BaseItemViewModel>).Cast<BaseItemViewModel>().Select(item => { item.IsChecked = true; return item; }).ToList();
                });

            UnselectAllCommand = new RelayCommand(
                p =>
                {
                    (p as IEnumerable<BaseItemViewModel>).Cast<BaseItemViewModel>().Select(item => { item.IsChecked = false; return item; }).ToList();
                });

            RemoveCacheCommand = new RelayCommand(
                async p =>
                {
                    var list = Cache.Where(item => item.IsChecked).ToList();
                    await Platform.RemoveCache(list.Select(item => { return item.Path; }).ToArray());
                    foreach (var item in list)
                    {
                        Cache.Remove(item);
                    }
                });

            RemoveInfoBaseCommand = new RelayCommand(
                async p =>
                {
                    var list = InfoBases.Where(item => item.IsChecked).ToList();
                    await Platform.RemoveInfoBases(list.Select(item => { return item.Name; }).ToArray());
                    foreach (var item in list)
                    {
                        InfoBases.Remove(item);
                    }
                });

        }

        private void Sort(ICollectionView view, string Name)
        {
            var sort = view.SortDescriptions.Select(item => item).Where(item => item.PropertyName == Name).FirstOrDefault();
            view.SortDescriptions.Clear();
            if (sort.PropertyName == null)
                view.SortDescriptions.Add(new SortDescription(Name, ListSortDirection.Ascending));
            else
                view.SortDescriptions.Add(
                    new SortDescription(
                        Name,
                        (sort.Direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }

        private async void Uninstall()
        {
            if (Status == Status.Uninstalling)
            {
                Status = Status.Idle;
                Progress = 0d;
                return;
            }
            else
                Status = Status.Uninstalling;

            var view = CollectionViewSource.GetDefaultView(InstalledVersions);
            view.Filter = x =>
            {
                var item = x as InstalledVersionItemViewModel;
                return item.IsChecked == true && Status == Status.Uninstalling;
            };

            int index = 0;
            List<InstalledVersionItemViewModel> list = InstalledVersions.Where(item => item.IsChecked).ToList();
            foreach (var item in list)
            {
                if (item.IsChecked && Status == Status.Uninstalling)
                {
                    var result = await Platform.Uninstall(item.UUID);
                    if (result)
                    {
                        InstalledVersions.Remove(item);
                        index++;

                        Progress = (double)index / (double)list.Count;
                    }
                }
            }

            view.Filter = null;
            Status = Status.Idle;
        }

        private async void PopulateInfoBasesAndCache()
        {
            InfoBasesArePopulating = true;
            InfoBases.Clear();
            CacheArePopulating = true;
            Cache.Clear();

            foreach (var item in await Platform.GetInfoBases())
            {
                InfoBases.Add(new InfoBaseItemViewModel()
                {
                    Name = item.Name,
                    UUID = item.UUID,
                    Size = item.Size,
                    Connection = item.Connection,
                    Version = item.Version
                });
            }

            foreach (var item in await Platform.GetCache())
            {
                Cache.Add(new CacheItemViewModel()
                {
                    Path = item.Path,
                    UUID = item.UUID,
                    Size = item.Size
                });
            }

            InfoBasesArePopulating = false;
            CacheArePopulating = false;

            foreach (var item in Cache)
            {
                item.Name = InfoBases.Where(i => i.UUID == item.UUID).FirstOrDefault()?.Name;
                item.IsChecked = string.IsNullOrEmpty(item.Name);
                item.Name = item.Name ?? "<База не найдена>";
            }
        }

        private async void PopulateInstalledVersions()
        {
            InstalledVersionsArePopulating = true;
            InstalledVersions.Clear();
            foreach (var item in await Platform.GetInstalledVersions())
            {
                InstalledVersions.Add(
                    new InstalledVersionItemViewModel()
                    {
                        Name = item.Name,
                        UUID = item.UUID,
                        Size = item.Size,
                        Version = item.VersionInt,
                        InstallDate = item.InstallDate
                    }
                );
            }
            InstalledVersionsArePopulating = false;
        }
    }
}
