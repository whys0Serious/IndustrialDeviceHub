using DeviceHub.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace DeviceHub.Client.Controls
{
    /// <summary>
    /// 全局异常处理
    /// 业务异常 显示消息给用户
    /// 系统异常 记日志+通用提示
    /// </summary>
    public static class GlobalExceptionHandler
    {
        private static readonly string LogDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Register()
        {
            Application.Current.DispatcherUnhandledException += OnUIException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainException;
            TaskScheduler.UnobservedTaskException += OnTaskException;
        }

        private static void OnUIException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Handle(e.Exception);
            e.Handled = true;   //阻止程序崩溃
        }

        private static void OnDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            Handle(e.ExceptionObject as Exception);
        }

        private static void OnTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Handle(e.Exception);
            e.SetObserved();
        }

        /// <summary>
        /// 统一处理：业务异常显示消息，系统异常记日志+通用提示
        /// </summary>
        private static void Handle(Exception? ex)
        {
            if (ex == null) return;

            // 业务异常：直接显示业务消息
            if (ex is BusinessException be)
            {
                AppMessageBox.ShowWarning(be.Message, "提示");
                return;
            }

            //系统异常:记日志+通用提示
            Log("系统异常", ex);
            AppMessageBox.ShowError(
                "操作失败，请稍后重试。\n\n如问题持续，请联系管理员。",
                "系统错误");
        }

        private static void Log(string source, Exception? ex)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                var file = Path.Combine(LogDirectory, $"error-{DateTime.Now:yyyyMMdd}.log");

                var sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine($"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"来源：{source}");
                sb.AppendLine($"类型：{ex?.GetType().FullName}");
                sb.AppendLine($"消息：{ex?.Message}");
                sb.AppendLine($"堆栈：{ex?.StackTrace}");
                sb.AppendLine();

                File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
            }
            catch
            {
                //日志失败不能影响主流程
            }
        }
    }
}
