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

namespace DeviceHub.Controls.Controls
{
    /// <summary>
    /// TitleBarControl.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBarControl : UserControl
    {
        public TitleBarControl()
        {
            InitializeComponent();
            // 监听窗口状态变化
            this.Loaded += TitleBarControl_Loaded;
        }
        private void TitleBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            var host = Window.GetWindow(this);
            if (host != null)
            {
                // 订阅窗口状态改变事件
                host.StateChanged += Host_StateChanged;

                // 页面加载时，立刻初始化一次图标状态
                UpdateMaximizeButtonState(host.WindowState);
            }
        }
        private void Host_StateChanged(object? sender, EventArgs e)
        {
            if (sender is Window host)
            {
                UpdateMaximizeButtonState(host.WindowState);
            }
        }

        private void UpdateMaximizeButtonState(WindowState state)
        {
            // 根据窗口状态，切换按钮的图标
            if (state == WindowState.Maximized)
            {
                // 最大化时，显示“还原”图标
                MaximizeButton.Content = new Path
                {
                    Data = (Geometry)FindResource("Icon_Restore"),
                    Stroke = (Brush)FindResource("TitleBarForegroundBrush"), // 使用你的前景色
                    StrokeThickness = 1.2,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                // 正常时，显示“最大化”图标
                MaximizeButton.Content = new Path
                {
                    Data = (Geometry)FindResource("Icon_Maximize"),
                    Stroke = (Brush)FindResource("TitleBarForegroundBrush"),
                    StrokeThickness = 1.2,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var host = Window.GetWindow(this);
            if (host == null) return;

            // 切换状态
            host.WindowState = host.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        public static readonly DependencyProperty WindowTitleProperty =
           DependencyProperty.Register(nameof(WindowTitle), typeof(string), typeof(TitleBarControl),
               new PropertyMetadata(string.Empty));

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }

        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register(nameof(LeftContent), typeof(object), typeof(TitleBarControl));

        public object LeftContent
        {
            get => GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register(nameof(RightContent), typeof(object), typeof(TitleBarControl));

        public object RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(nameof(ShowMinimize), typeof(bool), typeof(TitleBarControl),
                new PropertyMetadata(true));

        public bool ShowMinimize
        {
            get => (bool)GetValue(ShowMinimizeProperty);
            set => SetValue(ShowMinimizeProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeProperty =
            DependencyProperty.Register(nameof(ShowMaximize), typeof(bool), typeof(TitleBarControl),
                new PropertyMetadata(true));

        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        public static readonly DependencyProperty ShowCloseProperty =
            DependencyProperty.Register(nameof(ShowClose), typeof(bool), typeof(TitleBarControl),
                new PropertyMetadata(true));
        public static readonly DependencyProperty CornerRadiusProperty =
         DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
        typeof(TitleBarControl), new PropertyMetadata(new CornerRadius(12, 12, 0, 0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public bool ShowClose
        {
            get => (bool)GetValue(ShowCloseProperty);
            set => SetValue(ShowCloseProperty, value);
        }

        private Window Host => Window.GetWindow(this);

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => Host.WindowState = WindowState.Minimized;

/*        private void Maximize_Click(object sender, RoutedEventArgs e)
            => Host.WindowState = Host.WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;*/

        private void Close_Click(object sender, RoutedEventArgs e) => Host.Close();
    }
}
