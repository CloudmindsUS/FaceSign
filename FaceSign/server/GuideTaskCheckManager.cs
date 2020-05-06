using FaceSign.data;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FaceSign.server
{
    public class GuideTaskCheckManager
    {
        public static readonly GuideTaskCheckManager Instance = new GuideTaskCheckManager();

        private GuideTaskCheckManager()
        {
        }

        public void Start()
        {
            Task.Factory.StartNew(Check);

        }

        private void Check()
        {
            Thread.Sleep(30*1000);
            while (true) {
                var windowNameCN = "全自动红外热成像测温筛查系统";
                var windowNameEN = "IR Fever Warning System";
                var name = "ZS05A";
                if (BuildConfig.IRType.Equals(BuildConfig.IR_XT236))
                {
                    windowNameCN = "IR Fever Sensing System";
                    windowNameEN = "IR Fever Sensing System";
                    name = "S260.CoreUI";
                }
                else if (BuildConfig.IRType.Equals(BuildConfig.IR_M120))
                {
                    windowNameCN = "全自动红外热成像测温筛查系统";
                    windowNameEN = "IR Fever Warning System";
                    name = "M120";
                }
                var pr = Process.GetProcessesByName(name);
                var handleCN = Win32Api.FindWindow(null, windowNameCN);
                var handleEN = Win32Api.FindWindow(null, windowNameEN);
                var hasWindow = (handleCN != IntPtr.Zero || handleEN != IntPtr.Zero);
                if (pr != null && pr.Length > 0 && hasWindow)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}
