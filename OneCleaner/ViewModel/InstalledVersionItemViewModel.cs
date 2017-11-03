using System;

namespace OneCleaner
{
    public class InstalledVersionItemViewModel : BaseItemViewModel
    {
        public long Version { get; set; }
        public DateTime InstallDate { get; set; }
    }
}
