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
    /// RealtimeMonitorView.xaml 的交互逻辑
    /// </summary>
    public partial class RealtimeMonitorView : UserControl
    {
        public RealtimeMonitorView()
        {
            InitializeComponent();
            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is RealtimeMonitorViewModel vm)
                {
                    vm.Initialize();
                }
            };
            Unloaded += (_, _) =>
            {
                if (DataContext is RealtimeMonitorViewModel vm)
                {
                    vm.Dispose();
                }
            };
        }
    }
}
