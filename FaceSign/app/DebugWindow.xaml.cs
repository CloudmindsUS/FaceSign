using FaceSign.server;
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
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            MetalDetectionManager.Instance.OnDataReceived += Instance_OnDataReceived;
        }

        private void Instance_OnDataReceived(string data)
        {
            Dispatcher.Invoke(()=> {
                Label.Content = Label.Content + data + Environment.NewLine;
                Scroll.ScrollToEnd();
            });
        }

        private void OpenDoor_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.OpenDoor();
        }

        private void CloseDoor_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.CloseDoor();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Label.Content = "";
        }
    }
}
