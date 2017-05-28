namespace OneCleaner
{
    public class Cache
    {
        public string Path { get; set; }
        public string UUID { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", UUID, Size);
        }
    }
}
