using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCleaner.Platform
{
    public class CacheItem
    {
        public bool IsChecked { get; set; }
        public string Path { get; set; }
        public string UUID { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", UUID, Size);
        }
    }
}
