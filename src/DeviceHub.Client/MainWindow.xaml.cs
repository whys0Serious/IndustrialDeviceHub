
using DeviceHub.Controls.Controls;
using DeviceHub.Client.ViewModels;

namespace DeviceHub.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (_, _) => viewModel.Initialize();
        }
    }
}