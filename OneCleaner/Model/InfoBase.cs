namespace OneCleaner
{
    public class InfoBase
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        public string Version { get; set; }
        public long Size { get; set; }
        public string Connection { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
