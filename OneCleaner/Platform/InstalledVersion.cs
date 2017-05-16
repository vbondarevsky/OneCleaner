using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCleaner.Platform
{
    public class InstalledVersion
    {
        private string name;
        private string version;
        private string uuid;
        private string installLocation;
        private DateTime installDate;

        // TODO: Сделать свойства
        public long size;

        public InstalledVersion(string Name, string Version, string UUID, string InstallLocation, DateTime InstallDate, long Size)
        {
            this.name = Name;
            this.version = Version;
            this.uuid = UUID;
            this.installLocation = InstallLocation;
            this.installDate = InstallDate;
            this.size = Size;
        }
    }
}
