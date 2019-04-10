using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WCS.ExcuteJob
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        public App()
        {
            //this.Startup += new StartupEventHandler(App_Startup);
            //SingletonWindow.Process();
        }


        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "WCS.EXCUTEJOB", out ret);
            if (!ret)
            {
                MessageBox.Show("程序已在运行中...");
                Environment.Exit(0);
            }
            //保持互斥量不被垃圾回收器回收
            GC.KeepAlive(ret);
        }

    }

    public static class SingletonWindow
    {
        //注册附加属性
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SingletonWindow), new FrameworkPropertyMetadata(OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, Boolean value)
        {
            element.SetValue(IsEnabledProperty, value);
        }
        public static Boolean GetIsEnabled(DependencyObject element)
        {
            return (Boolean)element.GetValue(IsEnabledProperty);
        }

        //根据附加属性的返回值使能单实例模式
        public static void OnIsEnabledChanged(DependencyObject obj,DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue != true)
            {
                return;
            }

            Process();
            return;
        }

        public static void Process()    //如果不适用附加属性也可以直接使用此函数
        {
            //判断单实例的方式有很多，如mutex，process，文件锁等，这里用的是process方式

            var process = GetRunningInstance();
            if(process != null)
            {
                MessageBox.Show("程序已在运行中...","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
                HandleRunningInstance(process);
                Environment.Exit(0);
            }
        }

        const int WS_SHOWNORMAL = 1;

        [DllImport("User32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        static Process GetRunningInstance()
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") ==
                                           current.MainModule.FileName)
                    {
                        return process;
                    }
                }
                   
            }
            return null;
        }

        static void HandleRunningInstance(Process instance)
        {
            if (instance.MainWindowHandle != IntPtr.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    FlashWindow(instance.MainWindowHandle, 500);
                }

                SetForegroundWindow(instance.MainWindowHandle);
                ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            }
            else
            {
                //else 处理有点麻烦，简化如下
                MessageBox.Show("已经有一个实例在运行，无法启动第二个实例");
            }
        }

        static void FlashWindow(IntPtr hanlde, int interval)
        {
            FlashWindow(hanlde,true);
            Thread.Sleep(interval);
            FlashWindow(hanlde,false);
            Thread.Sleep(interval);
        }
    }
}
