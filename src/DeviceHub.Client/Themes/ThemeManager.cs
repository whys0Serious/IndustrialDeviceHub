using HandyControl.Expression.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DeviceHub.Client.Themes
{
    public static class ThemeManager
    {
        private const string ThemePrefix = "Brushes.";

        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        public static event EventHandler<AppTheme>? ThemeChanged;

        /// <summary>
        /// 应用主题
        /// </summary>
        public static void ApplyTheme(AppTheme theme)
        {
            CurrentTheme = theme;

            var uri = theme == AppTheme.Dark
                ? new Uri("pack://application:,,,/DeviceHub.Client;component/Themes/DarkBrushes.xaml")
                : new Uri("pack://application:,,,/DeviceHub.Client;component/Themes/LightBrushes.xaml");

            var dict = new ResourceDictionary { Source = uri };

            var appResources = Application.Current.Resources;

            //移除旧主题
            var old = appResources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains(ThemePrefix) == true);
            if (old != null)
                appResources.MergedDictionaries.Remove(old);

            //加入新主题
            appResources.MergedDictionaries.Add(dict);

            ThemeChanged?.Invoke(null, theme);
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        public static void ToggleTheme()
        {
            ApplyTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        }
    }
}
