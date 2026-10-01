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
    /// AlarmBanner.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmBanner : UserControl
    {
        public AlarmBanner()
        {
            InitializeComponent();
            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is AlarmBannerViewModel vm)
                    vm.Start();
            };

            Unloaded += (_, _) =>
            {
                if (DataContext is AlarmBannerViewModel vm)
                    vm.Dispose();
            };
        }
    }
}
