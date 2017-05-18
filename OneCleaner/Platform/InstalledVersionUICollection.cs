using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OneCleaner.Platform
{
    class InstalledVersionUICollection : ObservableCollection<InstalledVersionUI>
    {
        private static Task<List<InstalledVersionUI>> GetListVersions()
        {
            return Task.Run(() =>
            {
                List<InstalledVersionUI> list = new List<InstalledVersionUI>();

                var collection = new InstalledVersionCollection();
                foreach (var item in collection)
                {
                    var size = string.Format("{0} MB", item.Size / (1024 * 1024));
                    InstalledVersionUI instVerItem = new InstalledVersionUI(item.Name, item.Version, false, State.Installed, item.UUID, size);
                    list.Add(instVerItem);
                }


                return list;
            });
        }

        public async Task GetVersions()
        {

            this.ClearItems();

            var list = await GetListVersions();
            var sortedList = list.OrderBy(x => x.Version);
            foreach (var item in sortedList)
            {
                this.Add(item);
            }
            foreach (var item in this)
            {
                item.Index = this.IndexOf(item);
            }
        }
    }
}
