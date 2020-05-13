using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using FaceSign.log;
using FaceSign.data;
using System.Diagnostics;

namespace FaceSign.utils
{
    class Util
    {
        public static string GetIp() {
            string ip = "127.0.0.1";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress addr in host.AddressList)
            {
                if (addr.AddressFamily.ToString() == "InterNetwork")
                {
                    ip = addr.ToString();
                }
            }
            return ip;
        }

        public static string GetMac()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception)
            {
            }
            return "00-00-00-00-00-00";
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.I($@"get {fileName} md5 fail:{ex.Message}");
                return "";
            }
        }

        public static void StartIR()
        {
            if (!GuideAppIsRunning()) {
                var path = BuildConfig.GetIRExePath();
                Log.I("IR Path:" + path);
                var process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = true;
                try
                {
                    var result = process.Start();
                    Log.I("start result:" + result);
                }
                catch (Exception e)
                {
                    Log.I("start process fail:" + e.Message);
                }
            }            
        }

        public static bool GuideAppIsRunning()
        {
            var windowNameCN = "全自动红外热成像测温筛查系统";
            var windowNameEN = "IR Fever Warning System";
            var name = "ZS05A";
            if (BuildConfig.IRType.Equals(BuildConfig.IR_XT236))
            {
                windowNameCN = "IR Fever Sensing System";
                windowNameEN = "IR Fever Sensing System";
                name = "S260.CoreUI";
            }
            else if (BuildConfig.IRType.Equals(BuildConfig.IR_M120)) {
                windowNameCN = "全自动红外热成像测温筛查系统";
                windowNameEN = "IR Fever Warning System";
                name = "M120";
            }
            var processes = Process.GetProcessesByName(name);
            if (processes != null)
            {
                foreach (var ps in processes)
                {
                    if (ps.MainWindowTitle == windowNameCN || ps.MainWindowTitle == windowNameEN)
                    {
                        return true;
                    }
                }
            }            
            return false;
        }
    }
}
