using System.ComponentModel;
using System.Windows;

namespace place_block_wpf_ares.src.Views
{
    /// <summary>
    /// Interaction logic for ModaelssWpfDialog.xaml
    /// </summary>
    public partial class ModaelssWpfDialog : Window, INotifyPropertyChanged
    {
        public ModaelssWpfDialog()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
