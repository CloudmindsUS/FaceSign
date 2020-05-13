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
                if (Util.GuideAppIsRunning())
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
