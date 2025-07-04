using Microsoft.Win32;
using place_block_wpf_ares.Properties;
using place_block_wpf_ares.src.ViewModels;
using System.Windows;

namespace place_block_wpf_ares.src.Views
{
    /// <summary>
    /// Interaction logic for ModaelssWpfDialog.xaml
    /// </summary>
    public partial class ModaelssWpfDialog : Window
    {
        string coordPathFile;
        string blockPathFile;

        public ModaelssWpfDialog()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        //Dialoginteraktionen sollen in der UI-Ebene (View) stattfinden
        private void selCoordBtn_Click(object sender, RoutedEventArgs e)
        {
            var excelDlg = new OpenFileDialog();
            excelDlg.InitialDirectory = "c:\\"; //später umstellen
            excelDlg.Filter = "All files (*.*)|*.*|Excel files (*.xlsx)|*.xlsx";
            excelDlg.FilterIndex = 2;
            excelDlg.RestoreDirectory = true;

            if (excelDlg.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(excelDlg.FileName))
                    Settings.Default.LastCoordinatesPath = excelDlg.FileName;

                Settings.Default.Save();
                this.coordFilePath.Text = excelDlg.FileName;
                var fileStream = excelDlg.OpenFile();
            }
        }

        private void selBlockBtn_Click(object sender, RoutedEventArgs e)
        {
            var blockDlg = new OpenFileDialog();
            blockDlg.InitialDirectory = "c:\\";
            blockDlg.Filter = "All files (*.*)|*.*|DWG files (*.dwg)|*.dwg";
            blockDlg.FilterIndex = 2;
            blockDlg.RestoreDirectory = true;

            if (blockDlg.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(blockDlg.FileName))
                    Settings.Default.LastBlockPath = blockDlg.FileName;

                Settings.Default.Save();
                this.blockFilePath.Text = blockDlg.FileName;
                var fileStream = blockDlg.OpenFile();
            }
        }

        //public static RoutedCommand InsertCmd = new RoutedCommand();

        //private void InsertCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        //{ }

        //private void InsertCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    }
}
