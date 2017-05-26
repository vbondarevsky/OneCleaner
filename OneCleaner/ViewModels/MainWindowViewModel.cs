using OneCleaner.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCleaner
{
    public class MainWindowViewModel
    {
        public ObservableCollection<InstalledVersionItem> InstalledVersions { get; set; }
        public ObservableCollection<CacheItem> Cache { get; set; }
        public ObservableCollection<InfoBaseItem> InfoBases { get; set; }

        public MainWindowViewModel()
        {
            InstalledVersions = new ObservableCollection<InstalledVersionItem>
            {
                new InstalledVersionItem() { Name = "test" }
            };

            Cache = Platform.Platform.GetCache();
        }
    }
}
