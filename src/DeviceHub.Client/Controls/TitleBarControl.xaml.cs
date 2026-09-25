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

namespace DeviceHub.Client.Controls
{
    /// <summary>
    /// TitleBarControl.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBarControl : UserControl
    {
        public TitleBarControl()
        {
            InitializeComponent();
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

        public bool ShowClose
        {
            get => (bool)GetValue(ShowCloseProperty);
            set => SetValue(ShowCloseProperty, value);
        }

        private Window Host => Window.GetWindow(this);

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => Host.WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e)
            => Host.WindowState = Host.WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;

        private void Close_Click(object sender, RoutedEventArgs e) => Host.Close();
    }
}
