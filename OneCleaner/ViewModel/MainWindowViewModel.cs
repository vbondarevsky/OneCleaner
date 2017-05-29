using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace OneCleaner
{
    public class MainWindowViewModel : BaseViewModel
    {
        public Status Status { get; private set; }
        public double Progress { get; private set; }

        public ObservableCollection<InstalledVersionItemViewModel> InstalledVersions { get; private set; }
        public ObservableCollection<CacheItemViewModel> Cache { get; private set; }
        public ObservableCollection<InfoBaseItemViewModel> InfoBases { get; private set; }

        public ICommand UninstallCommand { get; set; }
        public ICommand SelectAllInstalledVersionCommand { get; set; }
        public ICommand UnselectAllInstalledVersionCommand { get; set; }

        public MainWindowViewModel()
        {
            Status = Status.Idle;

            InstalledVersions = new ObservableCollection<InstalledVersionItemViewModel>();
            PopulateInstalledVersions();

            Cache = new ObservableCollection<CacheItemViewModel>();
            PopulateCache();

            InfoBases = new ObservableCollection<InfoBaseItemViewModel>();
            PopulateInfoBases();

            UninstallCommand = new RelayCommand(Uninstall);

            SelectAllInstalledVersionCommand = new RelayCommand(
                () => { InstalledVersions.Select(item => { item.IsChecked = true; return item; }).ToList(); });

            UnselectAllInstalledVersionCommand = new RelayCommand(
                () => { InstalledVersions.Select(item => { item.IsChecked = false; return item; }).ToList(); });

            foreach (var item in Cache)
            {
                item.Name = InfoBases.Where(i => i.UUID == item.UUID).FirstOrDefault()?.Name;
            }
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

        private void PopulateInfoBases()
        {
            InfoBases.Clear();
            foreach (var item in Platform.GetInfoBases())
            {
                InfoBases.Add(new InfoBaseItemViewModel() { Name = item.Name, UUID = item.UUID, Size = item.Size, Connection = item.Connection });
            }
        }

        private void PopulateCache()
        {
            Cache.Clear();
            foreach (var item in Platform.GetCache())
            {
                Cache.Add(new CacheItemViewModel() { Path = item.Path, UUID = item.UUID, Size = item.Size });
            }
        }

        private void PopulateInstalledVersions()
        {
            InstalledVersions.Clear();
            foreach (var item in Platform.GetInstalledVersions())
            {
                InstalledVersions.Add(
                    new InstalledVersionItemViewModel()
                    {
                        Name = item.Name,
                        UUID = item.UUID,
                        Size = item.Size,
                        Version = item.Version,
                        InstallDate = item.InstallDate
                    }
                );
            }
        }
    }
}
