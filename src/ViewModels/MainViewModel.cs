namespace place_block_wpf_ares.src.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Binding properties 
        private string coordPath;
        private string blockPath;
        private string blockName;
        private string etageInput;
        private string reporter;

        public string CoordPath
        {
            get { return coordPath; }
            set { coordPath = value; }
        }
        public string BlockPath
        {
            get { return blockPath; }
            set { blockPath = value; }
        }
        public string BlockName
        {
            get { return blockName; }
            set { blockName = value; }
        }
        public string EtageInput
        {
            get { return etageInput; }
            set { etageInput = value; }
        }
        public string Reporter
        {
            get { return reporter; }
            set
            {
                reporter = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MainViewModel()
        {

        }



        //public Command BrowseCoordFile { get; }
    }
}
