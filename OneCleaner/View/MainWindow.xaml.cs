using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OneCleaner
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.Title = string.Format("{0} (DEBUG)", this.Title);
#endif
        }

        private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (BaseItemViewModel item in ((ListBox)(sender)).SelectedItems)
            {
                if (e.Key == Key.Space)
                    item.IsChecked = !item.IsChecked;
            }
        }
    }
}
