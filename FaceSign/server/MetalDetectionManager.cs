using FaceSign.data;
using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace FaceSign.server
{
    public class MetalDetectionManager
    {
        public static readonly MetalDetectionManager Instance = new MetalDetectionManager();
        public static readonly string DefaultComName = "COM1";
        public static readonly int DefaultQueryInterval = 100;
        public static readonly int DefaultAlarmThreshold = 900;
        public static readonly int DefaultQuietThreshold = 700;
        public static readonly int DefaultContinueCount = 3;
        public static readonly float DefaultAlarmTemperature = 37.5f;
        public static readonly int DefaultCloseDoorDelay = 500;
        public static readonly int DefaultOpenDoorDelay = 2000;

        public delegate void OnDataReceivedEvent(string data);

        public event OnDataReceivedEvent OnDataReceived;


        SerialTask SerialTask;

        public void Open()
        {
            Release();
            SerialTask = new SerialTask();
            SerialTask.OnDataReceived += SerialTask_OnDataReceived; ;
            SerialTask.Open();
        }

        private void SerialTask_OnDataReceived(string data)
        {
            OnDataReceived?.Invoke(data);
        }

        public void Release()
        {
            if (SerialTask != null)
            {
                SerialTask.Release();
                SerialTask.OnDataReceived -= SerialTask_OnDataReceived;
            }            
        }

        public void OpenDoor() {
            if (SerialTask != null)
            {
                SerialTask.OpenDoor();
            }
        }

        public void CloseDoor()
        {
            if (SerialTask != null)
            {
                SerialTask.CloseDoor();
            }
        }


        public bool HasAlarm()
        {
            if (SerialTask != null)
            {
                return SerialTask.HasAlarm;
            }
            return false;
        }

        public bool IsAlarmTemperature(float temperature)
        {
            if (SerialTask == null) return false;
            return SerialTask.IsAlarmTemperature(temperature);
        }




        public static string Read(string Key, string defaultText)
        {
            return SharePreference.Read(Keys.MetalDetectionSection,Key,defaultText);
        }

        public static int Read(string Key, int defaultValue)
        {
            var str = Read(Key, "" + defaultValue);
            int value = defaultValue;
            try
            {
                value = int.Parse(str);
            }catch(Exception e)
            {
                value = defaultValue;
            }
            return value;
        }

        public static float Read(string Key, float defaultValue)
        {
            var str = Read(Key, "" + defaultValue);
            float value = defaultValue;
            try
            {
                value = float.Parse(str);
            }
            catch (Exception e)
            {
                value = defaultValue;
            }
            return value;
        }

        public static long Write(string Key, string value)
        {
            return SharePreference.Write(Keys.MetalDetectionSection,Key,value);
        }

        public static long Write(string Key, int value)
        {
            return Write(Key,value+"");
        }

        
    }

    class SerialTask {
        private static readonly object Locker = new object();
        public delegate void OnDataReceivedEvent(string data);

        public event OnDataReceivedEvent OnDataReceived;
        private string ComName;
        private int QueryInterval;
        private int AlarmThreshold;
        private int QuietThreshold;
        private int MaxContinueCount;
        private int Count = 0;
        private int CloseDoorDelay = 0;
        private int OpenDoorDelay = 0;
        private SerialPort SerialPort;
        private bool IsRunning = true;
        private byte[] QueryCommand = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x84, 0x0A };
        private bool _HasAlarm = false;
        private float _AlarmTemperature;

        public bool HasAlarm { get => _HasAlarm; set => _HasAlarm = value; }
        public float AlarmTemperature { get => _AlarmTemperature; set => _AlarmTemperature = value; }
        private DateTime LastOpenDoorTime;
        private CancellationTokenSource CancellationToken;
        private Timer Timer;
        bool IsOpening;


        

        public SerialTask()
        {
        }



        public void Open()
        {
            Task.Factory.StartNew(StartCheck);
        }

        public void Release()
        {
            IsRunning = false;
            if (SerialPort != null)
            {
                SerialPort.Close();
                SerialPort = null;
            }
        }

        private void StartCheck()
        {
            Release();
            InitData();
            try
            {
                SerialPort = new SerialPort(ComName, 9600);
                if (!SerialPort.IsOpen)
                {
                    SerialPort.Open();
                }
                CloseDoor();
                QueryData();
            }
            catch (Exception e)
            {
                SerialPort = null;
                OnDataReceived?.Invoke("open serial port fail:" + e.Message);
                Log.I("open serial port fail:" + Environment.NewLine + e.StackTrace);
            }

        }



        private void QueryData()
        {
            while (IsRunning && SerialPort != null)
            {

                try
                {
                    SendData(QueryCommand);
                    var buffer = new byte[128];
                    var len = SerialPort.Read(buffer, 0, buffer.Length);
                    //var len = 7;
                    var data = new byte[len];
                    Array.ConstrainedCopy(buffer, 0, data, 0, data.Length);
                    var str = ByteUtil.BytesToString(data);
                    //Log.I("read data:"+str);
                    //OnDataReceived?.Invoke("read data:" + str);
                    if (data.Length == 7)
                    {
                        var crc = ByteUtil.GetCrc(data, data.Length - 2);
                        if (crc[0] == data[data.Length - 2] && crc[1] == data[data.Length - 1])
                        {
                            var hex = new byte[2];
                            hex[0] = data[3];
                            hex[1] = data[4];
                            var hexString = ByteUtil.BytesToString(hex);
                            var value = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
                            //Log.I("Noise level:" + value);
                            //OnDataReceived?.Invoke("Noise level:" + value);
                            if (value >= AlarmThreshold)
                            {
                                Count++;
                                OnDataReceived?.Invoke("探测到报警");
                            }
                            if (value <= QuietThreshold)
                            {
                                Count = 0;
                                if (_HasAlarm)
                                {
                                    OnDataReceived?.Invoke("报警解除");
                                }
                                _HasAlarm = false;
                            }
                            if (Count > MaxContinueCount)
                            {
                                _HasAlarm = true;
                                OnDataReceived?.Invoke("金属报警触发");
                                //CloseDoor();
                            }
                        }
                        else
                        {
                            Log.I("CRC check fail");
                            //OnDataReceived?.Invoke("CRC check fail");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.I("read data fail:" + Environment.NewLine + e.StackTrace);
                    OnDataReceived?.Invoke("read data fail:" + e.Message);
                }
                Thread.Sleep(QueryInterval);
            }
            Log.I("task is over");
        }

        public void SendData(byte[] data)
        {
            if (SerialPort == null)
            {
                Log.I("SerialPort is null can not send data");
                OnDataReceived?.Invoke("SerialPort is null can not send data");
                return;
            }
            //Log.I("ready send:" + ByteUtil.BytesToString(data));
            //OnDataReceived?.Invoke("ready send:" + ByteUtil.BytesToString(data));
            lock (Locker)
            {
                try
                {
                    SerialPort.Write(data, 0, data.Length);
                    Log.I("send:" + ByteUtil.BytesToString(data));
                    //OnDataReceived?.Invoke("send:" + ByteUtil.BytesToString(data));
                }
                catch (Exception e)
                {
                    Log.I($@"send data  {ByteUtil.BytesToString(data)}  fail:" + e.Message + Environment.NewLine + e.StackTrace);
                    OnDataReceived?.Invoke($@"send data  {ByteUtil.BytesToString(data)}  fail:" + e.Message);
                }
            }
        }

        public void OpenDoor()
        {
            if (IsOpening) {
                Log.I("is opening return");
                OnDataReceived?.Invoke("is opening return");
                return;
            }
            IsOpening = true;
            Timer = new Timer
            {
                AutoReset = false,
                Interval = OpenDoorDelay
            };
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();                            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte[] command = { 0xFF, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x99, 0xE4 };
            SendData(command);
            Log.I("open door");
            OnDataReceived?.Invoke("open door");
            Thread.Sleep(CloseDoorDelay);
            CloseDoor();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CloseDoor();
        }

        public void CloseDoor()
        {
            Log.I("Close Door");
            OnDataReceived?.Invoke("Close Door");
            byte[] command = { 0xFF, 0x05, 0x00, 0x00, 0x00, 0x00, 0xD8, 0x14 };
            SendData(command);
            LastOpenDoorTime = DateTime.Now;
            IsOpening = false;
        }

        private void InitData()
        {
            Count = 0;
            _HasAlarm = false;
            IsRunning = true;
            //LastOpenDoorTime = DateTime.Now;
            
            ComName = MetalDetectionManager.Read(Keys.KeyComName,MetalDetectionManager.DefaultComName);
            MetalDetectionManager.Write(Keys.KeyComName, ComName);
            QueryInterval = MetalDetectionManager.Read(Keys.KeyQueryInterval, MetalDetectionManager.DefaultQueryInterval);
            MetalDetectionManager.Write(Keys.KeyQueryInterval, QueryInterval);
            AlarmThreshold = MetalDetectionManager.Read(Keys.KeyAlarmThreshold, MetalDetectionManager.DefaultAlarmThreshold);
            MetalDetectionManager.Write(Keys.KeyAlarmThreshold, AlarmThreshold);
            QuietThreshold = MetalDetectionManager.Read(Keys.KeyQuietThreshold, MetalDetectionManager.DefaultQuietThreshold);
            MetalDetectionManager.Write(Keys.KeyQuietThreshold, QuietThreshold);
            MaxContinueCount = MetalDetectionManager.Read(Keys.KeyContinueCount, MetalDetectionManager.DefaultContinueCount);
            MetalDetectionManager.Write(Keys.KeyContinueCount, MaxContinueCount);
            _AlarmTemperature = MetalDetectionManager.Read(Keys.KeyAlarmTemperature, MetalDetectionManager.DefaultAlarmTemperature);
            CloseDoorDelay = MetalDetectionManager.Read(Keys.KeyCloseDoorDelay, MetalDetectionManager.DefaultCloseDoorDelay);
            MetalDetectionManager.Write(Keys.KeyCloseDoorDelay, CloseDoorDelay);
            OpenDoorDelay = MetalDetectionManager.Read(Keys.KeyOpenDoorDelay, MetalDetectionManager.DefaultOpenDoorDelay);
            MetalDetectionManager.Write(Keys.KeyOpenDoorDelay, OpenDoorDelay);
            Log.I($@"MetalDetectionManager Config,ComName:{ComName},QueryInterval:{QueryInterval}
                ,AlarmThreshold:{AlarmThreshold},QuietThreshold:{QuietThreshold}
                ,ContinueCount:{MaxContinueCount},AlarmTemperature:{AlarmTemperature}");
        }

        public bool IsAlarmTemperature(float temperature)
        {
            return temperature >= AlarmTemperature;
        }


    }
}
