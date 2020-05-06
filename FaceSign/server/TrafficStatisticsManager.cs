using Emgu.CV;
using Emgu.CV.Structure;
using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class TrafficStatisticsManager
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageData{
           public int h;
           public int w;
           public int c;
           [MarshalAs(UnmanagedType.ByValArray)]
           public float[] data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MotObject
        {
            public int track_id;
            public int x;
            public int y;
            public int w;
            public int h;
        }

        [DllImport("person_detect_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "detectorInitialize")]
        public extern static IntPtr detectorInitialize(string cfgFile, string weightsFile, int deviceID);

        [DllImport("person_detect_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runDetectorMat")]
        public extern static IntPtr runDetectorMat(IntPtr DectorPtr, IntPtr mat);

        [DllImport("person_detect_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runDetectorFile")]
        public extern static IntPtr runDetectorFile(IntPtr DectorPtr, char[] image);

        [DllImport("deepsort_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "moterInitialize")]
        public extern static IntPtr moterInitialize();

        [DllImport("deepsort_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runMoter")]
        public extern static IntPtr runMoter(IntPtr MoterPtr, IntPtr box, ImageData image, ref int len);

        [DllImport("deepsort_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runMoterFile")]
        public extern static IntPtr runMoterFile(IntPtr MoterPtr, IntPtr box, char[] image, ref int len);

        [DllImport("deepsort_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runMoterFileCount")]
        public extern static void runMoterFileCount(IntPtr MoterPtr, IntPtr box, char[] image, ref int len, ref int count);

        [DllImport("deepsort_dll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "runMoterMatCount")]
        public extern static void runMoterMatCount(IntPtr MoterPtr, IntPtr box, IntPtr mat, ref int len, ref int count);

        public delegate void ShowPersonCount(int count);

        public static readonly TrafficStatisticsManager Instance = new TrafficStatisticsManager();

        private TrafficStatisticsManager() { }
        private string cfgFile = $@"{FileUtil.GetAppRootPath()}\cfgs\yolov3.cfg";
        private string weightsFile = $@"{FileUtil.GetAppRootPath()}\cfgs\yolov3.weights";
        private readonly object locker = new object();
        public event ShowPersonCount OnPersonCountShow;

        IntPtr DectorPtr;
        IntPtr MoterPtr;
        int index;
        int Count=0;
        List<int> ids = new List<int>();
        private Capture capture;
        private string RtspUrl = "rtsp://admin:guide123@192.168.1.64:554";
        private bool IsDetect;
        private int ErrorCount;
        public int PersonCount;
        private DateTime LastGrabTime;
        private string tag= "TrafficStatisticsManager";

        public void Init()
        {
            DectorPtr = detectorInitialize(cfgFile,weightsFile,0);
            MoterPtr = moterInitialize();
            Log.I("DectorPtr:" + (DectorPtr == IntPtr.Zero));
            Log.I("MoterPtr:" + (MoterPtr == IntPtr.Zero));
            Task.Factory.StartNew(StartCapture);
            Task.Factory.StartNew(Check);
        }

        private void Check()
        {
            while (true)
            {
                if (capture == null || LastGrabTime == null)
                {
                    ErrorCount++;
                }
                var now = DateTime.Now;
                int time = (int)now.Subtract(LastGrabTime).TotalMilliseconds;
                if (ErrorCount > 10 || time > 10 * 1000)
                {
                    Log.I(tag,"ready restart");
                    ErrorCount = 0;
                    LastGrabTime = DateTime.Now;
                    if (capture != null)
                    {
                        capture.ImageGrabbed -= Capture_ImageGrabbed;
                        capture.Stop();
                    }
                    Task.Factory.StartNew(StartCapture);
                }
                Thread.Sleep(1000);
            }
        }

        private void StartCapture()
        {
            LastGrabTime = DateTime.Now;
            capture = new Capture(RtspUrl);
            capture.ImageGrabbed += Capture_ImageGrabbed;
            capture.Start();
            Log.I(tag,"start capture");
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            LastGrabTime = DateTime.Now;
            Emgu.CV.Mat mat = new Emgu.CV.Mat();
            capture.Retrieve(mat);
            if (IsDetect) return;
            IsDetect = true;
            Task.Factory.StartNew(() => {
                IntPtr imagePtr = mat.Ptr;
                var boxPtr = runDetectorMat(DectorPtr, imagePtr);
                int len = 0;
                int count = 0;
                runMoterMatCount(MoterPtr, boxPtr, imagePtr, ref len, ref count);
                Console.WriteLine("当前帧人数:" + len + ",总人数:" + count);
                PersonCount = count;
                OnPersonCountShow?.Invoke(PersonCount);
                IsDetect = false;
            });
        }
    }
}
