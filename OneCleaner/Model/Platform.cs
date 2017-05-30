using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneCleaner
{
    public class Platform
    {
        public static Task<List<Cache>> GetCache()
        {
            return Task.Run(() =>
            {
                List<Cache> cache = new List<Cache>();

                var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                var directory = Path.Combine(localAppData, "1C");

                Regex reg = new Regex(@"[0-9a-zA-F]{8}-[0-9a-zA-F]{4}-[0-9a-zA-F]{4}-[0-9a-zA-F]{4}-[0-9a-zA-F]{12}$");
                foreach (var path in Directory.GetDirectories(directory, "1cv8*"))
                {
                    var dirs = Directory.GetDirectories(path).Where(p => reg.IsMatch(p)).ToList();

                    foreach (var dir in dirs)
                    {
                        cache.Add(new Cache() { Path = dir, UUID = Path.GetFileName(dir), Size = Utility.GetDirectorySize(dir) });
                    }
                }

                return cache;
            });
        }

        public static Task<List<InstalledVersion>> GetInstalledVersions()
        {
            return Task.Run(() =>
            {
                List<InstalledVersion> installedVersions = new List<InstalledVersion>();

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
                            Size = Utility.GetDirectorySize(location)
                        };
                        installedVersions.Add(instVerItem);
                    }
                }

                return installedVersions;
            });
        }

        public static Task<bool> Uninstall(string uuid)
        {
            return Task.Run(() =>
            {
                try
                {
                    object[] args = new object[0];

#if DEBUG
                    var result = 0;
                    System.Threading.Thread.Sleep(5000);
#else
                    if (string.IsNullOrEmpty(uuid))
                        return false;


                    var ProcessInfo = new System.Diagnostics.ProcessStartInfo("msiexec.exe", String.Format("/x{0} /q", uuid))
                    {
                        CreateNoWindow = false,
                        UseShellExecute = true
                    };
                    var Process = System.Diagnostics.Process.Start(ProcessInfo);
                    Process.WaitForExit();
                    var result = Process.ExitCode;
                    Process.Close();

#endif

                    if (result == 0)
                        return true;
                    else
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public static Task<List<InfoBase>> GetInfoBases()
        {
            return Task.Run(() =>
            {
                List<InfoBase> infoBases = new List<InfoBase>();
                string ibases = GetInfoBasesPath();

                var parser = new FileIniDataParser();
                parser.Parser.Configuration.AllowDuplicateSections = true;
                parser.Parser.Configuration.AllowDuplicateKeys = true;

                IniData data = parser.ReadFile(ibases);

                InfoBase infoBase;

                foreach (var section in data.Sections)
                {
                    if (section.Keys["Connect"] != null)
                    {
                        infoBase = new InfoBase()
                        {
                            Name = section.SectionName,
                            Connection = section.Keys["Connect"],
                            UUID = section.Keys["ID"]
                        };
                        string path;
                        if (infoBase.Connection.StartsWith("File"))
                        {
                            path = infoBase.Connection.Substring(6, infoBase.Connection.Length - 8);
                            if (Directory.Exists(path))
                                infoBase.Size = Utility.GetDirectorySize(path);
                        }
                        infoBases.Add(infoBase);
                    }
                }

                return infoBases;
            });
        }

        private static string GetInfoBasesPath()
        {
            var localAppData = Environment.GetEnvironmentVariable("APPDATA");
            var ibases = Path.Combine(localAppData, "1C", "1CEStart", "ibases.v8i");
            if (!File.Exists(ibases))
                throw new FileNotFoundException("Файл не найден", ibases);

            return ibases;
        }

        public static Task RemoveInfoBases(string[] infoBases)
        {
            return Task.Run(() =>
            {
                var file_backup = $"ibases.v8i_backup_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
                File.Copy(GetInfoBasesPath(), Path.Combine(Path.GetDirectoryName(GetInfoBasesPath()), file_backup));

                var parser = new FileIniDataParser();
                parser.Parser.Configuration.AllowDuplicateSections = true;
                parser.Parser.Configuration.AllowDuplicateKeys = true;
                parser.Parser.Configuration.AssigmentSpacer = "";

                IniData data = parser.ReadFile(GetInfoBasesPath(), Encoding.UTF8);

                foreach (var infoBase in infoBases)
                {
                    data.Sections.RemoveSection(infoBase);
                }

                parser.WriteFile(GetInfoBasesPath(), data, new UTF8Encoding(true));
            });

        }

        private static bool IsPlatform1C(string vendor)
        {
            return vendor == "1С-Софт" || vendor == "1C-Soft" || vendor == "1C" || vendor == "1С";
        }
    }
}
