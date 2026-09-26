using DeviceHub.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceHub.Client.Views
{
    /// <summary>
    /// DeviceListView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceListView : UserControl
    {
        private readonly DeviceListViewModel _viewModel;

        public DeviceListView()
        {
            InitializeComponent();
            // DataContext由父ContentControl 自动传入（CurrentViewModel）
            DataContextChanged += async (_, e) =>
            {
                if (e.NewValue is DeviceListViewModel vm)
                {
                    await vm.InitializeAsync();
                }
            };
        }
    }
}
