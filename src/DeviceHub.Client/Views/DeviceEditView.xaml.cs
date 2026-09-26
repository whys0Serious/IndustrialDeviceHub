using DeviceHub.Client.Controls;
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
using System.Windows.Shapes;

namespace DeviceHub.Client.Views
{
    /// <summary>
    /// DeviceEditView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceEditView : CustomWindow
    {
        public DeviceEditView()
        {
            InitializeComponent();

            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is DeviceEditViewModel vm)
                {
                    vm.RequestClose += OnRequestClose;
                }
            };

            Closed += (_, _) =>
            {
                if (DataContext is DeviceEditViewModel vm)
                {
                    vm.RequestClose -= OnRequestClose;
                }
            };
        }

        private void OnRequestClose(bool result)
        {
            DialogResult = result;
        }
    }
}
