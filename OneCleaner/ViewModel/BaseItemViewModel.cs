namespace OneCleaner
{
    public class BaseItemViewModel : BaseViewModel
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }
    }
}
