using FaceSign.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FaceSign.app
{
    public class TestApp : Application
    {
        [STAThread]
        static void Main()
        {
            // 定义Application对象作为整个应用程序入口 
            TestApp app = new TestApp();
            app.Run();
        }
        private HttpListener listener;

        public TestApp()
        {
            Log.Init();
            Task.Run(()=> {
                Test();
            });
        }

        private void Test()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($@"http://localhost:9090/");
            listener.Start();
            Task.Run(()=> {
                while (true) {
                    var ctx = listener.GetContext();
                    var url = ctx.Request.RawUrl;
                    Log.I("Receive:" + url);
                }
            });
            int index = 0;
            while (true)
            {
                index++;
                Task.Run(() => {
                    var req = (HttpWebRequest)WebRequest.Create($@"http://localhost:9090/"+index);
                    Log.I("Send:" + index);
                    req.Method = WebRequestMethods.Http.Get;
                    req.AllowAutoRedirect = true;
                    req.KeepAlive = false;
                    req.Accept = "*/*";
                    var res = (HttpWebResponse)req.GetResponse();
                });
            }
        }

        private void Receive(IAsyncResult result)
        {
            var ctx = listener.EndGetContext(result);
            listener.BeginGetContext(Receive, null);
            var url = ctx.Request.RawUrl;
            Log.I("Receive:"+url); 
        }
    }
}
