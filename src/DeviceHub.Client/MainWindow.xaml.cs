
using DeviceHub.Client.ViewModels;
using DeviceHub.Controls.Controls;
using System.ComponentModel;

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