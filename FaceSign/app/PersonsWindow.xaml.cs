using FaceSign.data;
using FaceSign.http;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.server;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;

namespace FaceSign.app
{
    /// <summary>
    /// PersonsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PersonsWindow : Window
    {
        MemoryStream MMS;
        DispatcherTimer Timer;
        Label PersonInfo;
        Image Avater;
        string terminalId;

        public PersonsWindow(string id)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            Closed += PersonsWindow_Closed;
            InitializeComponent();
            terminalId = id;
            if (BuildConfig.IRType == BuildConfig.IR_G120
                || BuildConfig.IRType == BuildConfig.IR_M120)
            {
                G120.Visibility = Visibility.Visible;
                PersonInfo = G120PersonInfo;
                Avater = G120Avater;
            }
            else {
                XT236.Visibility = Visibility.Visible;
                PersonInfo = XT236PersonInfo;
                Avater = XT236Avater;
            }
            if (BuildConfig.IsSupportAccessControl)
            {
                KeyDown += PersonsWindow_KeyDown;
            }
            if (BuildConfig.IsSupportTrafficStatistics)
            {
                PersonCount.Visibility = Visibility.Visible;
            }
            InitOrBindDeviceAsync();
            OpenIR();
            //if (BuildConfig.IsSupportAccessControl)
            //{
            //    new SerialPortWindow().Show();
            //    new DebugWindow().Show();
            //}
        }

        private void PersonsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                new SerialPortWindow().Show();
            }else if (e.Key == Key.F2)
            {
                new DebugWindow().Show();
            }
        }

        private void OpenIR()
        {
            Util.StartIR();
            GuideTaskCheckManager.Instance.Start();
        }

        private async void InitOrBindDeviceAsync()
        {
            var hardkey = SharePreference.Read(Keys.KeyHardkey, "");
            if (string.IsNullOrEmpty(hardkey)) {
                hardkey = terminalId;
                SharePreference.Write(Keys.KeyHardkey, terminalId);
                var request = new BindRequest
                {
                    hardkey = hardkey,
                    terminal_id = terminalId
                };
                var response = await ApiService.Bind(request);
                Log.I("BIND DEVICE:"+ response.code);
            }
            var req = new InitRequest
            {
                hardkey = hardkey
            };
            var rep = await ApiService.Init(req);
            Log.I("Init Device:"+rep.code);
            DownloadPersonServer.GetInstance().Start(terminalId);
            DownloadPersonServer.GetInstance().onUpdatePerson += PersonsWindow_onUpdatePerson;
            HttpWebServer.Instance.OnPersonShow += Instance_OnPersonShow;
            TrafficStatisticsManager.Instance.OnPersonCountShow += Instance_OnPersonCountShow;
            HttpWebServer.Instance.Start(terminalId);
            CheckUpdateServer.GetInstance().Start(terminalId);
        }

        private void Instance_OnPersonCountShow(int count)
        {
            Dispatcher.Invoke(() => {
                PersonCount.Content = R.Str.txt_pass_person_count+count;
            });
        }

        private void PersonsWindow_onUpdatePerson(bool doing)
        {
            Dispatcher.Invoke(() =>
            {
                if (doing)
                {
                    PersonInfo.Foreground = new SolidColorBrush(Colors.White);
                    if (BuildConfig.IRType == BuildConfig.IR_G120
                    ||BuildConfig.IRType==BuildConfig.IR_M120)
                    {
                        PersonInfo.Content = $@"{R.Str.update_face_lib}{System.Environment.NewLine}{R.Str.pause_face_recognition}";
                    }
                    else {
                        PersonInfo.Content = $@"{R.Str.update_face_lib},{R.Str.pause_face_recognition}";
                    }
                }
                else
                {
                    PersonInfo.Content = "";
                }
            });
        }

        private void PersonsWindow_Closed(object sender, EventArgs e)
        {
            PersonInfo.Content = "";
            Avater.Source = null;
            MMS?.Close();
            MMS?.Dispose();
            Timer?.Stop();
        }

        private System.Windows.Media.Color GetColor(System.Drawing.Color drawColor)
        {
            return System.Windows.Media.Color.FromArgb(drawColor.A, drawColor.R, drawColor.G, drawColor.B);
        }

        private void Instance_OnPersonShow(model.PersonModel person)
        {
            Dispatcher.Invoke(()=> {
                if (Timer != null&&Timer.IsEnabled) {
                    Timer.Stop();
                }
                if (person.type == "8" && BuildConfig.IsSupportBlacklist)
                {
                    PersonInfo.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    PersonInfo.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BuildConfig.IRType == BuildConfig.IR_XT236)
                {
                    PersonInfo.Content = $@"{person.name} {person.temperature.ToString("f1")}℃";
                }
                else {
                    PersonInfo.Content = $@"{person.name}{System.Environment.NewLine}{person.temperature.ToString("f1")}℃";
                }
                Avater.Source = GetBitmap(person.RealTimeFace);                
                CreateTimer();
            });
        }

        private void CreateTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(5);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(()=>{
                PersonInfo.Content = "";
                Avater.Source = null;
                MMS?.Close();
                MMS?.Dispose();
                MMS = null;
                Timer.Stop();
            });
        }



        private ImageSource GetBitmap(string realTimeFace)
        {
            BitmapImage Bitmap = null;
            try
            {
                MMS?.Close();
                MMS?.Dispose();
                byte[] buffer = Convert.FromBase64String(realTimeFace);
                MMS = new MemoryStream(buffer);
                Bitmap = new BitmapImage();
                Bitmap.BeginInit();
                Bitmap.StreamSource = MMS;
                Bitmap.EndInit();
            }
            catch (Exception e)
            {
                Log.I("load face fail:" + e.Message);
            }
            return Bitmap;
        }
    }
}
