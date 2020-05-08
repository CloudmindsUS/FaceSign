using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class RtspManager
    {
        public static readonly RtspManager Instance = new RtspManager();

        public void Start()
        {
            //Task.Factory.StartNew(StartNginx);
        }

        private void StartNginx()
        {
            var process = Process.GetProcessesByName("nginx");
            if (process != null)
            {
                foreach(var p in process)
                {
                    p.Kill();
                }
            }
        }
    }
}
