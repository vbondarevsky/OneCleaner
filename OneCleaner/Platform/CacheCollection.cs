using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneCleaner.Platform
{
    public class CacheCollection : ObservableCollection<CacheItem>
    {
        public CacheCollection()
        {

            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            var directory = Path.Combine(localAppData, "1C");

            Regex reg = new Regex(@"[0-9a-zA-F]{8}-[0-9a-zA-F]{4}-[0-9a-zA-F]{4}-[0-9a-zA-F]{4}-[0-9a-zA-F]{12}$");
            foreach (var path in Directory.GetDirectories(directory, "1cv8*"))
            {
                var dirs = Directory.GetDirectories(path).Where(p => reg.IsMatch(p)).ToList();

                foreach (var dir in dirs)
                {
                    this.Add(new CacheItem() { Path=dir, UUID = Path.GetFileName(dir), Size = DirectorySize(new DirectoryInfo(path))});
                }
            }
        }

        private long DirectorySize(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetFiles().Sum(file => file.Length) +
                   directoryInfo.GetDirectories().Sum(directory => DirectorySize(directory));
        }
    }
}
