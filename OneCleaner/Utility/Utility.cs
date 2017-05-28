using System.IO;
using System.Linq;

namespace OneCleaner
{
    public class Utility
    {
        public static long GetDirectorySize(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);
            return directoryInfo.GetFiles().Sum(file => file.Length) +
                   directoryInfo.GetDirectories().Sum(dir => GetDirectorySize(dir.FullName));
        }
    }
}
