using OneCleaner.Platform;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;

namespace OneCleaner
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string TotalSize { get; set; }
        public string FreeSize { get; set; }

        private InstalledVersionUICollection _installedVersions = new InstalledVersionUICollection();
        private bool _cancelFlag = false;

        private CacheCollection _cacheCollection;
        public MainWindow()
        {
            InitializeComponent();

            _cacheCollection = new CacheCollection();
            //List.ItemsSource = _installedVersions;

            //MainGrid.DataContext = this;

            TotalSize = "0 MB";
            FreeSize = "0 MB";


#if DEBUG
            this.Title = this.Title + " (DEBUG)";
#endif
        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            List.Visibility = Visibility.Hidden;
            ButtonUninstall.IsEnabled = false;

            await _installedVersions.GetVersions();

            List.Visibility = Visibility.Visible;
            ButtonUninstall.IsEnabled = true;
        }

        private void SetWindowStateIdle()
        {
            ButtonUninstall.IsEnabled = true;
            ButtonUninstall.Content = "Выполнить удаление";

            ButtonCancel.Visibility = Visibility.Collapsed;
            ButtonCancel.Content = "Отменить";
            ButtonCancel.IsEnabled = true;
            var view = CollectionViewSource.GetDefaultView(_installedVersions);
            view.Filter = null;
        }

        private void SetWindowStateInProgress()
        {
            ButtonUninstall.IsEnabled = false;
            ButtonCancel.Visibility = Visibility.Visible;
            ButtonUninstall.Content = "Удаление...";
            var view = CollectionViewSource.GetDefaultView(_installedVersions);
            view.Filter = x =>
            {
                var item = x as InstalledVersionUI;
                return item.State == State.MarkedForUninstall || item.State == State.Uninstalling;
            };
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancelFlag = true;
            ButtonCancel.IsEnabled = false;
            ButtonCancel.Content = "Отмена...";
            var itemsToCancel = _installedVersions.Where(item => item.State == State.MarkedForUninstall);
            foreach (var item in itemsToCancel)
            {
                item.State = State.Installed;
            }
            var view = CollectionViewSource.GetDefaultView(_installedVersions);
            view.Refresh();
        }


        private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (InstalledVersionUI item in ((ListBox)(sender)).SelectedItems)
            {
                if (e.Key == Key.Space)
                {
                    item.IsChecked = !item.IsChecked;
                }
            }
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _installedVersions)
            {
                item.IsChecked = true;
            }
        }

        private void ButtonUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _installedVersions)
            {
                item.IsChecked = false;
            }
        }

        private async void ButtonUninstall_Click(object sender, RoutedEventArgs e)
        {
            List<InstalledVersionUI> uninst = _installedVersions
                .Where(item => item.IsChecked && item.State == State.Installed)
                .Select(item => { item.State = State.MarkedForUninstall; return item; })
                .ToList();

            SetWindowStateInProgress();

            int ind = 0;
            this.TaskbarItemInfo.ProgressValue = 0d;
            this.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

            foreach (var item in uninst)
            {
                if (_cancelFlag)
                {
                    _cancelFlag = false;
                    break;
                }

                item.State = State.Uninstalling;
                this.TaskbarItemInfo.Description = item.Name + " (версия: " + item.Version + ")";

                var result = await item.Uninstall();
                if (result)
                {
                    _installedVersions.Remove(item);
                    ind++;
                    this.TaskbarItemInfo.ProgressValue = (double)ind / (double)uninst.Count;
                }
            }

            this.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            SetWindowStateIdle();
        }
    }
}
