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

namespace FaceSign.server
{
    public class AccessControlManager
    {
        public static readonly AccessControlManager Instance = new AccessControlManager();
        private static readonly string DefaultComName = "COM2";

        private string AccessControlComName;
        private SerialPort SerialPort;

        private AccessControlManager()
        {
            InitData();
            try
            {
                SerialPort = new SerialPort(AccessControlComName, 9600);
                if (!SerialPort.IsOpen)
                {
                    SerialPort.Open();
                }
                Task.Factory.StartNew(ReceiveData);
            }
            catch (Exception e)
            {
                SerialPort = null;
                Log.I("open ac serial port fail:" + Environment.NewLine + e.StackTrace);
            }

        }

        private void ReceiveData()
        {
            while (SerialPort != null)
            {
                try
                {
                    var buffer = new byte[128];
                    var len = SerialPort.Read(buffer, 0, buffer.Length);
                    var data = new byte[len];
                    Array.ConstrainedCopy(buffer, 0, data, 0, data.Length);
                    Log.I("read ac data:" + ByteUtil.BytesToString(data));
                }
                catch (Exception e)
                {
                    Log.I("read ac data fail:" + Environment.NewLine + e.StackTrace);
                }
                Thread.Sleep(500);
            }
        }

        public void OpenDoor()
        {
            if (SerialPort == null)
            {
                Log.I("serial port is null can not open door");
            }
            else
            {
                try
                {
                    byte[] command = { 0xFF, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x99, 0xE4 };
                    SerialPort.Write(command, 0, command.Length);
                }
                catch(Exception e)
                {
                    Log.I("open door fail:"+e.Message+Environment.NewLine+e.StackTrace);
                }
            }
        }

        public void CloseDoor()
        {
            if (SerialPort == null)
            {
                Log.I("serial port is null can not close door");
            }
            else
            {
                try
                {
                    byte[] command = { 0xFF, 0x05, 0x00, 0x00, 0x00, 0x00, 0xD8, 0x14 };
                    SerialPort.Write(command, 0, command.Length);
                }
                catch (Exception e)
                {
                    Log.I("close door fail:" + e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        private void InitData()
        {
            AccessControlComName = Read(Keys.KeyAccessControlComName, DefaultComName);
            Write(Keys.KeyAccessControlComName,AccessControlComName);
        }

        public static string Read(string Key, string defaultText)
        {
            return SharePreference.Read(Keys.AccessControlSection, Key, defaultText);
        }

        public static int Read(string Key, int defaultValue)
        {
            var str = Read(Key, "" + defaultValue);
            int value = defaultValue;
            int.TryParse(str, out value);
            return value;
        }

        public static long Write(string Key, string value)
        {
            return SharePreference.Write(Keys.AccessControlSection, Key, value);
        }

        public static long Write(string Key, int value)
        {
            return Write(Key, value + "");
        }


    }
}
