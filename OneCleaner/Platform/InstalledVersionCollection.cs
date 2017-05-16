using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCleaner.Platform
{
    public class InstalledVersionCollection
    {
        private List<InstalledVersion> list = new List<InstalledVersion>();

        public InstalledVersionCollection()
        {
            populateList();
        }

        private void populateList()
        {
            list.Clear();

            var subkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (var itemUUID in subkey.GetSubKeyNames())
            {
                var itemKey = subkey.OpenSubKey(itemUUID);
                string name = (string)itemKey.GetValue("DisplayName", null);
                if (name != null)
                {
                    string vendor = (string)itemKey.GetValue("Publisher", null);
                    if (vendor == "1С-Софт" || vendor == "1C" || vendor == "1С")
                    {
                        var version = (string)itemKey.GetValue("DisplayVersion", "0.0.0.0");
                        var location = (string)itemKey.GetValue("InstallLocation", "");
                        var dateStr = (string)itemKey.GetValue("InstallDate", "00000000");
                        var date = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                        var directory = new DirectoryInfo(location);
                        InstalledVersion instVerItem = new InstalledVersion(name, version, itemUUID, location, date, DirectorySize(directory));
                        list.Add(instVerItem);
                    }
                }

            }

        }

        private long DirectorySize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) +
                   dir.GetDirectories().Sum(di => DirectorySize(di));
        }

        public long GetTotalSize()
        {
            return this.list.Sum(item => item.size);
        }
    }
}
