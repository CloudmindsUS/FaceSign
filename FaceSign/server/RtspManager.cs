using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class RtspManager
    {
        public static readonly RtspManager Instance = new RtspManager();

        public void Start()
        {
            Task.Factory.StartNew(StartNginx);
            Task.Factory.StartNew(StartVLHlsServer);
            Task.Factory.StartNew(StartIRHlsServer);
        }

        private void StartIRHlsServer()
        {
            StartHlsServer("ir",8554);
        }

        private void StartVLHlsServer()
        {
            StartHlsServer("vl", 8555);
        }

        private void StartHlsServer(string name, int port)
        {
            Log.I($@"check {name} port");
            while (!PortInUse(port))
            {
                Thread.Sleep(1000);
            }
            Log.I("ir rtsp server start,ready transport to hls");
            var process = new Process();
            var ffmpeg = Path.Combine(FileUtil.GetAppRootPath(), $@"hls_server\ffmpeg.exe");
            var m3u8 = Path.Combine(FileUtil.GetAppRootPath(), $@"hls_server\html\hls\{name}.m3u8");
            process.StartInfo.FileName = ffmpeg;
            process.StartInfo.Arguments = $@" -i rtsp://admin:9999@127.0.0.1:{port}/video/1 " +
                "-fflags flush_packets -max_delay 1 " +
                "-an -flags -global_header " +
                "-hls_time 1 -hls_list_size 1 -hls_wrap 3 " +
                $@"-vcodec copy -y {m3u8}";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = true;
            try
            {
                var result = process.Start();
                Log.I($@"start {name} hls server result:" + result);
            }
            catch (Exception e)
            {
                Log.I($@"start {name} hls server fail:" + e.Message);
            }
        }

        

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.I("Process_ErrorDataReceived:" + e.Data);

        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.I("Process_OutputDataReceived:"+e.Data);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Log.I("vl exit");
        }

        private void StartNginx()
        {
            var ps = Process.GetProcessesByName("nginx");
            if (ps != null && ps.Length > 0) {
                Log.I("nginx is start");
                return;
            }
            var process = new Process();
            process.StartInfo.FileName = $@"start_nginx.bat";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = true;
            try
            {
                var result = process.Start();
                Log.I("start nginx result:" + result);
            }
            catch (Exception e)
            {
                Log.I("start nginx process fail:" + e.Message);
            }
        }

        private  bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            { 
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
    }
}
