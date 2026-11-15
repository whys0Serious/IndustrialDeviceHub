using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DeviceHub.Controls.Behaviors
{
    public static class DataGridSelectAllBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DataGridSelectAllBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        public static readonly DependencyProperty TargetButtonProperty =
            DependencyProperty.RegisterAttached("TargetButton", typeof(Button), typeof(DataGridSelectAllBehavior), new PropertyMetadata(null));

        public static Button GetTargetButton(DependencyObject obj) => (Button)obj.GetValue(TargetButtonProperty);
        public static void SetTargetButton(DependencyObject obj, Button value) => obj.SetValue(TargetButtonProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid) return;

            if ((bool)e.NewValue)
            {
                dataGrid.Loaded += DataGrid_Loaded;

                //使用 DependencyPropertyDescriptor 监听 ItemsSource 的变化
                var descriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
                descriptor?.AddValueChanged(dataGrid, DataGrid_ItemsSourceChanged);
            }
            else
            {
                dataGrid.Loaded -= DataGrid_Loaded;
                var descriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
                descriptor?.RemoveValueChanged(dataGrid, DataGrid_ItemsSourceChanged);
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                EnsureSelectAllColumn(dataGrid);
                UpdateButtonState(dataGrid);
            }
        }

        private static void DataGrid_ItemsSourceChanged(object? sender, EventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                EnsureSelectAllColumn(dataGrid);
                UpdateButtonState(dataGrid);

                // 当数据源变化时（翻页/刷新），强制把表头的全选框重置为 false
                if (dataGrid.Columns.Count > 0 && dataGrid.Columns[0].Header is CheckBox headerCheckBox)
                {
                    headerCheckBox.IsChecked = false;
                }
            }
        }

        private static void EnsureSelectAllColumn(DataGrid dataGrid)
        {
            if (dataGrid.Columns.Count > 0 && dataGrid.Columns[0] is DataGridTemplateColumn col && col.Width.Value == 50)
                return;

            // 表头
            var headerCheckBox = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerCheckBox.Checked += (_, _) => { ToggleAllRows(dataGrid, true); UpdateButtonState(dataGrid); };
            headerCheckBox.Unchecked += (_, _) => { ToggleAllRows(dataGrid, false); UpdateButtonState(dataGrid); };



            // 单元格
            var cellTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler((s, _) => UpdateButtonState(dataGrid)));
            factory.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler((s, _) => UpdateButtonState(dataGrid)));
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetBinding(ToggleButton.IsCheckedProperty, new System.Windows.Data.Binding("IsSelected")
            {
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            // 监听单行勾选，刷新导出按钮
            factory.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler((s, _) => UpdateButtonState(dataGrid)));
            factory.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler((s, _) => UpdateButtonState(dataGrid)));

            cellTemplate.VisualTree = factory;

            var selectColumn = new DataGridTemplateColumn
            {
                Width = new DataGridLength(50),
                Header = headerCheckBox,
                CellTemplate = cellTemplate
            };
            dataGrid.Columns.Insert(0, selectColumn);
        }

        private static void ToggleAllRows(DataGrid dataGrid, bool isSelected)
        {
            if (dataGrid.ItemsSource is IEnumerable items)
            {
                foreach (var item in items)
                {
                    var prop = item.GetType().GetProperty("IsSelected");
                    prop?.SetValue(item, isSelected);
                }
            }
            UpdateButtonState(dataGrid); // 全选/反选后刷新按钮
        }

        private static void UpdateButtonState(DataGrid dataGrid)
        {
            var button = GetTargetButton(dataGrid);
            if (button == null) return;

            bool hasSelected = false;
            if (dataGrid.ItemsSource is IEnumerable items)
            {
                foreach (var item in items)
                {
                    var prop = item.GetType().GetProperty("IsSelected");
                    if (prop != null && (bool)prop.GetValue(item)!)
                    {
                        hasSelected = true;
                        break;
                    }
                }
            }
            button.Dispatcher.Invoke(() => button.IsEnabled = hasSelected);
        }

        public static void ResetHeaderState(DataGrid dataGrid)
        {
            if (dataGrid.Columns.Count > 0 && dataGrid.Columns[0].Header is CheckBox headerCb)
            {
                headerCb.IsChecked = false;
            }
        }
    }
}