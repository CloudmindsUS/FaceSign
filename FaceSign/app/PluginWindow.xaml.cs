using FaceSign.log;
using FaceSign.utils;
using FaceSign.widget;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceSign.app
{
    /// <summary>
    /// BottomBarWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PluginWindow : Window
    {
        Process exeProcess;
        string exePath = $@"App_ZS05A.exe";

        public PluginWindow()
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.CanMinimize;
            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            InitializeComponent();
            Loaded += PluginWindow_Loaded;
            Closed += PluginWindow_Closed;
        }

        private void PluginWindow_Closed(object sender, EventArgs e)
        {
            if (exeProcess != null)
                try
                {
                    exeProcess.Kill();
                }
                catch (Exception ex) { }
        }

        private void PluginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogoWindow logo = new LogoWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            logo.Show();
            if (File.Exists(exePath))
            {
                Log.I("准备启动测温程序");
                exeProcess = new Process();
                exeProcess.StartInfo.FileName = "App_ZS05A.exe";
                exeProcess.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                exeProcess.StartInfo.CreateNoWindow = true;//不显示程序窗口
                exeProcess.Start();//启动程序
                while (exeProcess.MainWindowHandle == IntPtr.Zero)
                {
                    Thread.Sleep(5);
                }
                var parent = (new WindowInteropHelper(this)).Handle;
                var child = Win32Api.FindWindow(null, "全自动红外热成像测温告警系统");
                Win32Api.SetParent(child, parent);
            }
            else {
                Log.I("软件不存在");
            }
            
        }
        
    }
}
