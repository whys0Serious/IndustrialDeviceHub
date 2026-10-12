using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace DeviceHub.Controls.Controls
{
    /// <summary>
    /// 自定义窗口基类
    /// 主窗体：直角 + AllowsTransparency=false + WindowChrome（拖动/最大化/调整大小）
    /// 弹窗：圆角 + 阴影 + AllowsTransparency=true + WindowChrome（拖动，不调整大小）
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
            ResizeMode = ResizeMode.NoResize;

            //默认按主窗体：非透明 + 背景色
            AllowsTransparency = false;
            SetResourceReference(BackgroundProperty, "BackgroundBrush");
            ApplyMainWindowChrome();

            DataContextChanged += OnDataContextChanged;
            Closed += OnWindowClosed;
        }

        // IsDialog：区分主窗体 / 弹窗
        public static readonly DependencyProperty IsDialogProperty =
            DependencyProperty.Register(nameof(IsDialog), typeof(bool),
                typeof(CustomWindow), new PropertyMetadata(false, OnIsDialogChanged));

        public bool IsDialog
        {
            get => (bool)GetValue(IsDialogProperty);
            set => SetValue(IsDialogProperty, value);
        }

        private static void OnIsDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CustomWindow w) return;

            if ((bool)e.NewValue)
            {
                //弹窗：透明 + 保留 WindowChrome（拖动 + 双击），禁用调整大小
                w.AllowsTransparency = true;
                w.Background = Brushes.Transparent;
                w.ShowInTaskbar = false;

                WindowChrome.SetWindowChrome(w, new WindowChrome
                {
                    CaptionHeight = 32,
                    ResizeBorderThickness = new Thickness(0),
                    GlassFrameThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(0),
                    UseAeroCaptionButtons = false
                });
            }
            else
            {
                //主窗体：非透明 + BackgroundBrush + 完整 Chrome
                w.AllowsTransparency = false;
                w.SetResourceReference(BackgroundProperty, "BackgroundBrush");
                w.ApplyMainWindowChrome();
            }
        }

        private void ApplyMainWindowChrome()
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                CaptionHeight = 32,
                ResizeBorderThickness = new Thickness(6),
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            });
        }

        // 圆角
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
                typeof(CustomWindow), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        // 阴影开关
        public static readonly DependencyProperty HasShadowProperty =
            DependencyProperty.Register(nameof(HasShadow), typeof(bool),
                typeof(CustomWindow), new PropertyMetadata(false));

        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }

        // 标题栏按钮显隐
        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(nameof(ShowMinimize), typeof(bool),
                typeof(CustomWindow), new PropertyMetadata(true));

        public bool ShowMinimize
        {
            get => (bool)GetValue(ShowMinimizeProperty);
            set => SetValue(ShowMinimizeProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeProperty =
            DependencyProperty.Register(nameof(ShowMaximize), typeof(bool),
                typeof(CustomWindow), new PropertyMetadata(true));

        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        public static readonly DependencyProperty ShowCloseProperty =
            DependencyProperty.Register(nameof(ShowClose), typeof(bool),
                typeof(CustomWindow), new PropertyMetadata(true));

        public bool ShowClose
        {
            get => (bool)GetValue(ShowCloseProperty);
            set => SetValue(ShowCloseProperty, value);
        }

        // 标题栏左右自定义内容
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

        // IDialogAware 订阅
        private IDialogAware? _dialogAware;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_dialogAware != null)
            {
                _dialogAware.RequestClose -= OnRequestClose;
                _dialogAware = null;
            }

            if (e.NewValue is IDialogAware aware)
            {
                _dialogAware = aware;
                _dialogAware.RequestClose += OnRequestClose;
            }
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            if (_dialogAware != null)
            {
                _dialogAware.RequestClose -= OnRequestClose;
                _dialogAware = null;
            }
        }

        private void OnRequestClose(bool result)
        {
            try { DialogResult = result; }
            catch { Close(); }
        }
    }
}