using FaceSign.data;
using FaceSign.db;
using FaceSign.http;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.model;
using FaceSign.server;
using FaceSign.utils;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FaceSign.app
{
    public class FaceSignApp : Application
    {
        FaceSignApp() {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = null;
            if (e.ExceptionObject is Exception) {
                exception = e.ExceptionObject as Exception;
            } 
            Log.I("Crash DispatcherUnhandledException:" + exception?.Message + "StackTrace:\r\n" + exception?.StackTrace);

        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            
            Log.I("Crash DispatcherUnhandledException:"+e.Exception.Message+ "StackTrace:\r\n" + e.Exception.StackTrace);
        }

        System.Threading.Mutex mutex;


        [STAThread]
        static void Main()
        {
            // 定义Application对象作为整个应用程序入口            
            FaceSignApp app = new FaceSignApp();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            BuildConfig.Init();
            Application.Current.Resources.MergedDictionaries
                .Add(R.Resource);            
            mutex = new System.Threading.Mutex(true, "ElectronicNeedleTherapySystem", out bool ret);
            if (!ret) {
                Util.StartIR();
                Application.Current.Shutdown();
                return;
            }
            else
            {
                Log.Init();
                ShutdownMode = ShutdownMode.OnLastWindowClose;
                if (BuildConfig.Debug)
                {
                    if (BuildConfig.IsSupportMetalDetection)
                    {
                        new SerialPortWindow().Show();
                    }
                }
                else
                {
                    if (BuildConfig.IsSupportTrafficStatistics)
                    {
                        TrafficStatisticsManager.Instance.Init();
                    }
                    if (BuildConfig.IsSupportAI)
                    {
                        Log.I("ready init");
                        FaceManager.Instance.Init();
                        var list = DBManager.Instance.DB.Queryable<PersonModel>().ToList();
                        Log.I("Current People:" + list.Count);
                        var terminalId = SharePreference.Read(Keys.KeyTerminalId, "");
                        Log.I("login ID:" + terminalId);
                        if (string.IsNullOrEmpty(terminalId))
                        {
                            var window = new MainWindow();
                            window.Show();
                        }
                        else
                        {
                            PersonsWindow window = new PersonsWindow(terminalId);
                            window.Show();
                        }
                    }
                    else
                    {
                        Util.StartIR();
                        GuideTaskCheckManager.Instance.Start();
                    }
                    LogoWindow logo = new LogoWindow();
                    logo.Show();
                    if (BuildConfig.IsSupportMetalDetection)
                    {
                        MetalDetectionManager.Instance.Open();
                    }
                    
                }
                
            }
                  
        }

        private  void Test()
        {
            var path = $@"{FileUtil.GetFaceDirPath()}\1955.jpg";
            var feature=FaceManager.Instance.GetFaceFeature(path);
            var list = DBManager.Instance.DB.Queryable<PersonModel>().ToList();
            float core = 0;
            PersonModel verifyPerson = null;
            foreach (PersonModel person in list)
            {
                var personFeature = JsonConvert.DeserializeObject<float[]>(person.feature);
                var smiliaty = FaceManager.Instance.GetFaceSmiliaty(feature, personFeature);
                if (smiliaty > core)
                {
                    core = smiliaty;
                    verifyPerson = person;
                }
            }
            Log.I($@"置信度:{core},最相似的人员是:{verifyPerson.name}");
        }

        private string GetBase64(string person_id)
        {
            var path = $@"{FileUtil.GetFaceDirPath()}\{person_id}.jpg";
            var fs = new FileStream(path,FileMode.Open);
            var buffer = new byte[fs.Length];
            fs.Read(buffer,0,buffer.Length);
            return Convert.ToBase64String(buffer);
        }
    }
}
