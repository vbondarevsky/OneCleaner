using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OneCleaner.Platform
{
    public class InstalledVersionCollection : List<InstalledVersion>
    {
        public long TotalSize
        {
            get
            {
                long totalSize = 0;
                HashSet<string> locations = new HashSet<string>();
                foreach (var item in this)
                {
                    if (!locations.Contains(item.Location))
                    {
                        totalSize += item.Size;
                        locations.Add(item.Location);
                    }
                }
                return totalSize;
            }
        }

        public InstalledVersionCollection()
        {
            Populate();
        }

        private void Populate()
        {
            this.Clear();

            var key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var keyUninstall32 = localMachine32.OpenSubKey(key);

            var keysUninstall = new List<RegistryKey>() { keyUninstall32 };
            if (Environment.Is64BitOperatingSystem)
            {
                var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var keyUninstall64 = localMachine64.OpenSubKey(key);
                keysUninstall.Add(keyUninstall64);
            }

            foreach (var keyUninstall in keysUninstall)
            {
                foreach (var itemUUID in keyUninstall.GetSubKeyNames())
                {
                    var itemKey = keyUninstall.OpenSubKey(itemUUID);
                    if (itemKey == null)
                        continue;

                    var name = (string)itemKey.GetValue("DisplayName", null);
                    var vendor = (string)itemKey.GetValue("Publisher", null);
                    var version = (string)itemKey.GetValue("DisplayVersion", "0.0.0.0");
                    var location = (string)itemKey.GetValue("InstallLocation", "");
                    var dateStr = (string)itemKey.GetValue("InstallDate", "00010101");

                    if (name == null || !IsPlatform1C(vendor) || String.IsNullOrEmpty(location))
                        continue;

                    InstalledVersion instVerItem = new InstalledVersion
                    {
                        Name = name,
                        Version = version,
                        UUID = itemUUID,
                        Location = location,
                        InstallDate = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture),
                        Size = DirectorySize(new DirectoryInfo(location))
                    };
                    this.Add(instVerItem);
                }
            }
        }

        private static bool IsPlatform1C(string vendor)
        {
            return vendor == "1С-Софт" || vendor == "1C-Soft" || vendor == "1C" || vendor == "1С";
        }

        private long DirectorySize(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetFiles().Sum(file => file.Length) +
                   directoryInfo.GetDirectories().Sum(directory => DirectorySize(directory));
        }

    }
}
