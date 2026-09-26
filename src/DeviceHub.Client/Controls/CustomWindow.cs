using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shell;

namespace DeviceHub.Client.Controls
{
    /// <summary>
    /// 自定义窗口头控件
    /// </summary>
    public class CustomWindow : Window
    {
        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CustomWindow),
                new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        public CustomWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = false;
            // 默认：不可调整大小（对话框行为）
            ResizeMode = ResizeMode.NoResize;

            var chrome = new WindowChrome
            {
                CaptionHeight = 32,
                ResizeBorderThickness = new Thickness(6),
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);
        }

        // ---------- 标题栏按钮显隐 ----------

        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(nameof(ShowMinimize), typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowMinimize
        {
            get => (bool)GetValue(ShowMinimizeProperty);
            set => SetValue(ShowMinimizeProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeProperty =
            DependencyProperty.Register(nameof(ShowMaximize), typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(false));

        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        public static readonly DependencyProperty ShowCloseProperty =
            DependencyProperty.Register(nameof(ShowClose), typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowClose
        {
            get => (bool)GetValue(ShowCloseProperty);
            set => SetValue(ShowCloseProperty, value);
        }

        // ---------- 标题栏左右自定义内容 ----------

        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register(nameof(LeftContent), typeof(object), typeof(CustomWindow));

        public object? LeftContent
        {
            get => GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register(nameof(RightContent), typeof(object), typeof(CustomWindow));

        public object? RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }
    }
}
