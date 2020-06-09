﻿using FaceSign.data;
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
using System.Media;
using System.Runtime.InteropServices;
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
using System.Windows.Interop;
//using System.Drawing;

namespace FaceSign.app
{
    /// <summary>
    /// PersonsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PersonsWindow : Window
    {
        MemoryStream MMS;
        DispatcherTimer Timer;
        DispatcherTimer StopWarningTimer;
        DispatcherTimer FahrenheitTimer;
        DispatcherTimer RgbimageTimer;

        SoundPlayer WarningPlayer;
        Label PersonInfo;
        Image Avater;
        //Image rgbImageLoaded;
        string terminalId;

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

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
                //rgbImageLoaded = Image_Loaded;
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
            WarningPlayer = new SoundPlayer("warning.wav");
            InitOrBindDeviceAsync();
            OpenIR();
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
            Log.I("IInit Device:"+rep.code);
            if (BuildConfig.IsSupportAI)
            {
                DownloadPersonServer.GetInstance().Start(terminalId);
                DownloadPersonServer.GetInstance().onUpdatePerson += PersonsWindow_onUpdatePerson;
            }
            HttpWebServer.Instance.OnPersonShow += Instance_OnPersonShow;
            HttpWebServer.Instance.OnFahrenheitShow += Instance_OnFahrenheitShow;
            HttpWebServer.Instance.OnImageLoaded += Instance_OnImageLoaded;
            //HttpWebServer.Instance.OnControlWin += Instance_OnControlWin;
            TrafficStatisticsManager.Instance.OnPersonCountShow += Instance_OnPersonCountShow;
            HttpWebServer.Instance.Start(terminalId);
            CheckUpdateServer.GetInstance().Start(terminalId);
        }

        //private void Instance_OnControlWin(int count)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        Log.I("接收到的数据：" + count);
        //    });

        //}


        private void Instance_OnFahrenheitShow(model.IsAlarmPointModel model)
        {            
            Dispatcher.Invoke(() =>
            {
                if (Fahrenheit.Visibility == Visibility.Hidden)
                {
                    Fahrenheit.Visibility = Visibility.Visible;
                    Fahrenheit_ir.Visibility = Visibility.Visible;
                }
                if (FahrenheitTimer != null)
                {
                    FahrenheitTimer.Tick -= FahrenheitTimer_Tick;
                    FahrenheitTimer.Stop();
                }
                if (model.temperature > 37)
                {
                    Fahrenheit.Background = Brushes.Red;
                    Fahrenheit_ir.Background = Brushes.Red;

                } else
                {
                    Fahrenheit.Background = Brushes.Blue;
                    Fahrenheit_ir.Background = Brushes.Blue;
                }

                Fahrenheit.Margin = new Thickness(960 - 127 + model.X, 5 + model.Y, 0, 0);
                Fahrenheit_ir.Margin = new Thickness(50 + model.X, 5 + model.Y, 0, 0);

                float tmp = (model.temperature * 9) / 5 + 32;
                Fahrenheit.Content = " " +  tmp.ToString("F1") + "F°";
                Fahrenheit_ir.Content = " " + tmp.ToString("F1") + "F°";
                FahrenheitTimer = new DispatcherTimer();
                FahrenheitTimer.Tick += FahrenheitTimer_Tick;
                FahrenheitTimer.Interval = TimeSpan.FromSeconds(1);
                FahrenheitTimer.Start();

            });
        }

        private void FahrenheitTimer_Tick(object sender, EventArgs e)
        {
            Fahrenheit.Visibility = Visibility.Hidden;
            Fahrenheit_ir.Visibility = Visibility.Hidden;
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
            StopWarningTimer?.Stop();
            FahrenheitTimer?.Stop();
            RgbimageTimer?.Stop();
            if (WarningPlayer != null)
            {
                WarningPlayer.Stop();
            }
        }

        private System.Windows.Media.Color GetColor(System.Drawing.Color drawColor)
        {
            return System.Windows.Media.Color.FromArgb(drawColor.A, drawColor.R, drawColor.G, drawColor.B);
        }
        private void Instance_OnImageLoaded(model.IsAlarmEventModel alarm_model)
        {
            Dispatcher.Invoke(() => {
                if (G120_bgr_stream.Visibility == Visibility.Hidden)
                {
                    G120_bgr_stream.Visibility = Visibility.Visible;
                    G120_infrard_stream.Visibility = Visibility.Visible;
                }
                if (RgbimageTimer != null)
                {
                    RgbimageTimer.Tick -= RgbimageTimer_Tick;
                    RgbimageTimer.Stop();
                }
                //if (RgbimageTimer != null && RgbimageTimer.IsEnabled)
                //{
                //    RgbimageTimer.Stop();
                //}
                if (BuildConfig.IRType == BuildConfig.IR_G120)
                {
                    System.Drawing.Bitmap rgbimage;
                    System.Drawing.Bitmap infraredimage;
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(alarm_model.visibleimg)))
                    {
                        rgbimage = new System.Drawing.Bitmap(ms);
                        IntPtr hBitmap = rgbimage.GetHbitmap();
                        Image_Loaded.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(alarm_model.infraredimg)))
                    {
                        infraredimage = new System.Drawing.Bitmap(ms);
                        IntPtr hBitmap = infraredimage.GetHbitmap();
                        Infrard_Stream_Image_Loaded.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    RgbimageTimer = new DispatcherTimer();
                    RgbimageTimer.Tick += RgbimageTimer_Tick;
                    RgbimageTimer.Interval = TimeSpan.FromSeconds(1);
                    RgbimageTimer.Start();
                }


            });

        }
        private void RgbimageTimer_Tick(object sender, EventArgs e)
        {
            G120_bgr_stream.Visibility = Visibility.Hidden;
            G120_infrard_stream.Visibility = Visibility.Hidden;
        }

        private void Instance_OnPersonShow(model.PersonModel person)
        {
            Dispatcher.Invoke(()=> {
                if (Timer != null&&Timer.IsEnabled) {
                    Timer.Stop();
                }
                if (StopWarningTimer != null && StopWarningTimer.IsEnabled)
                {
                    StopWarningTimer.Stop();
                }
                if (person.type == "8" && BuildConfig.IsSupportBlacklist)
                {
                    PersonInfo.Foreground = new SolidColorBrush(Colors.Red);
                    WarningPlayer.PlayLooping();
                    StopWarningTimer = new DispatcherTimer();
                    StopWarningTimer.Interval = TimeSpan.FromSeconds(5);
                    StopWarningTimer.Tick += StopWarningTimer_Tick;
                    StopWarningTimer.Start();
                }
                else
                {
                    PersonInfo.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BuildConfig.IRType == BuildConfig.IR_XT236)
                {
                    //PersonInfo.Content = $@"{person.name} {person.temperature.ToString("f1")}℃";
                    PersonInfo.Content = $@"{person.name} {(person.temperature * 1.8 + 32).ToString("f1")}°F";

                }
                if (BuildConfig.IRType == BuildConfig.IR_G120)
                {
                    //PersonInfo.Content = $@"{person.name} {person.temperature.ToString("f1")}℃";
                    PersonInfo.Content = $@"{person.name} {(person.temperature * 1.8 + 32).ToString("f1")}°F";

                }
                else {
                    PersonInfo.Content = $@"{person.name}{System.Environment.NewLine}{(person.temperature * 1.8 + 32).ToString("f1")}°F";
                }
                Avater.Source = GetBitmap(person.RealTimeFace);                
                //CreateTimer();
            });
        }

        private void StopWarningTimer_Tick(object sender, EventArgs e)
        {
            WarningPlayer.Stop();
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
    public class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private IntPtr _MainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._MainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }
}
