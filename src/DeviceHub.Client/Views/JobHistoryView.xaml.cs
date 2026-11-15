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
    /// JobHistoryView.xaml 的交互逻辑
    /// </summary>
    public partial class JobHistoryView : UserControl
    {
        public JobHistoryView()
        {
            InitializeComponent();
            DataContextChanged += async (_, e) =>
            {
                if (e.NewValue is JobHistoryViewModel vm)
                {
                    await vm.InitializeAsync();
                }
            };
            this.Loaded += (s, e) =>
            {
                if (DataContext is JobHistoryViewModel vm)
                {
                    // 监听翻页或刷新动作
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(vm.CurrentPageIndex))
                        {
                            //每次页码变化，强制重置表头复选框
                            if (HistoryGrid.Columns.Count > 0 && HistoryGrid.Columns[0].Header is CheckBox cb)
                            {
                                cb.IsChecked = false;
                            }
                        }
                    };
                }
            };
        }
    }
}
