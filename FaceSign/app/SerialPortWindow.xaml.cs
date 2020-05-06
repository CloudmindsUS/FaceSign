using FaceSign.data;
using FaceSign.log;
using FaceSign.server;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// SerialPortWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SerialPortWindow : Window
    {
        public SerialPortWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            var names = SerialPort.GetPortNames();
            if (names != null&&names.Length>0)
            {
                var selectName = MetalDetectionManager.Read(Keys.KeyComName,"");
                for(var i=0;i<names.Length;i++)
                {
                    ComName.Items.Add(names[i]);
                    if (selectName == names[i])
                    {
                        ComName.SelectedIndex = i;
                    }
                }
                ComName.SelectionChanged += ComName_SelectionChanged;
            }
            SaveButton.Click += SaveButton_Click;
            QueryInterval.Text = MetalDetectionManager.Read(Keys.KeyQueryInterval, MetalDetectionManager.DefaultQueryInterval).ToString();
            AlarmThreshold.Text = MetalDetectionManager.Read(Keys.KeyAlarmThreshold, MetalDetectionManager.DefaultAlarmThreshold).ToString();
            QuietThreshold.Text = MetalDetectionManager.Read(Keys.KeyQuietThreshold, MetalDetectionManager.DefaultQuietThreshold).ToString();
            ContinueCount.Text = MetalDetectionManager.Read(Keys.KeyContinueCount, MetalDetectionManager.DefaultContinueCount).ToString();
            AlarmTemperature.Text = MetalDetectionManager.Read(Keys.KeyAlarmTemperature, MetalDetectionManager.DefaultAlarmTemperature).ToString();
            CloseDoorDelay.Text = MetalDetectionManager.Read(Keys.KeyCloseDoorDelay, MetalDetectionManager.DefaultCloseDoorDelay).ToString();
            OpenDoorDelay.Text = MetalDetectionManager.Read(Keys.KeyOpenDoorDelay, MetalDetectionManager.DefaultOpenDoorDelay).ToString();
        }

        private void ComName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Regex.IsMatch(QueryInterval.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_query_interval_error);
                return;
            }
            if (!Regex.IsMatch(AlarmThreshold.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_alarm_threshold_error);
                return;
            }
            if (!Regex.IsMatch(QuietThreshold.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_quiet_threshold_error);
                return;
            }
            if (!Regex.IsMatch(ContinueCount.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_continue_count_error);
                return;
            }
            if(!Regex.IsMatch(AlarmTemperature.Text, "^([0-9]{1,}[.]{0,1}[0-9]*)$"))
            {
                MessageBox.Show(R.Str.txt_alarm_temperature_error);
                return;
            }
            if (!Regex.IsMatch(CloseDoorDelay.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_close_door_delay_error);
                return;
            }
            if (!Regex.IsMatch(OpenDoorDelay.Text, "^\\d{1,}$"))
            {
                MessageBox.Show(R.Str.txt_open_door_delay_error);
                return;
            }
            var item = ComName.SelectedItem.ToString();
            MetalDetectionManager.Write(Keys.KeyComName, item);
            MetalDetectionManager.Write(Keys.KeyQueryInterval, QueryInterval.Text);
            MetalDetectionManager.Write(Keys.KeyAlarmThreshold, AlarmThreshold.Text);
            MetalDetectionManager.Write(Keys.KeyQuietThreshold, QuietThreshold.Text);
            MetalDetectionManager.Write(Keys.KeyContinueCount, ContinueCount.Text);
            MetalDetectionManager.Write(Keys.KeyAlarmTemperature, AlarmTemperature.Text);
            MetalDetectionManager.Write(Keys.KeyCloseDoorDelay, CloseDoorDelay.Text);
            MetalDetectionManager.Write(Keys.KeyOpenDoorDelay, OpenDoorDelay.Text);
            MetalDetectionManager.Instance.Open();
        }

        private void Instance_OnDataReceived(string data)
        {
            Dispatcher.Invoke(()=> {
                //Message.Text += data + Environment.NewLine;
            });
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.Open();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.Release();

        }

        private void OpenDoorBtn_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.OpenDoor();
        }

        private void CloseDoorBtn_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.CloseDoor();
            byte[] data = { 0x01, 0x03,0x02,0x01,0xF6,0x39,0x92 };
            var crc = ByteUtil.GetCrc(data,data.Length-2);
            if (crc[0] == data[data.Length - 2] && crc[0] == data[data.Length - 1])
            {
                Log.I("crc fail");
            }
            var value = ByteUtil.BytesToString(crc);
            Log.I(value + "");
        }

        private void OpenDoorButton_Click(object sender, RoutedEventArgs e)
        {
            MetalDetectionManager.Instance.OpenDoor();
        }
    }
}
