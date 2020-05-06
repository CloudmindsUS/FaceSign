using FaceSign.data;
using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceSign.app
{
    /// <summary>
    /// LogoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogoWindow : Window
    {
        public LogoWindow()
        {            
            InitializeComponent();
            Topmost = true;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            if (BuildConfig.IRType == BuildConfig.IR_G120
                ||BuildConfig.IRType.Equals(BuildConfig.IR_M120))
            {
                if (!BuildConfig.IsSupportAI) {
                    G120.Visibility = Visibility.Visible;
                }
            }
            else
            {
                XT236.Visibility = Visibility.Visible;
            }

        }

        private void LogoWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void LogoWindow_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = false;
        }
    }
}
