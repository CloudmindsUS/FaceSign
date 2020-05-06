using FaceSign.data;
using FaceSign.http;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class CheckUpdateServer
    {
        private static readonly CheckUpdateServer instance = new CheckUpdateServer();
        private readonly int PollingTime = 10*1000;

        private CheckUpdateServer()
        {
        }

        public static CheckUpdateServer GetInstance()
        {
            return instance;
        }

        private string terminalId;
        private CancellationToken token;
        private bool isCheckUpdate;

        public void Start(string terminalId)
        {
            this.terminalId = terminalId;
            token = new CancellationToken();
            Task.Factory.StartNew(() => {
                while (!token.IsCancellationRequested)
                {
                    CheckSoftwareUpdateAsync();
                    Thread.Sleep(PollingTime);
                }
            });
        }

        private async void CheckSoftwareUpdateAsync()
        {
            if (isCheckUpdate) return;
            isCheckUpdate = true;
            var req = new RefreshRequest
            {
                hardkey = terminalId,
                terminal_id = terminalId,
                human_traffic = TrafficStatisticsManager.Instance.PersonCount
            };
            var rep = await ApiService.Refresh(req);
            if (rep.version_code <= BuildConfig.VersionCode)
            {
                isCheckUpdate = false;
                return;
            }
            Log.I("it need update to:" + rep.version_code);
            var path = $@"{FileUtil.GetUpdatePath()}\{GetNameByUrl(rep.url)}";
            var result = HttpManager.DownloadFileByBreakpoint(rep.url, path);
            if (!result)
            {
                isCheckUpdate = false;
                return;
            }
            var md5 = Util.GetMD5HashFromFile(path);
            if (!rep.md5.Equals(md5))
            {
                FileInfo file = new FileInfo(path);
                file.Delete();
                isCheckUpdate = false;
                return;
            }
            var process = new Process();
            process.StartInfo.Arguments = path;
            process.StartInfo.FileName = "update.bat";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                process.Start();
            }
            catch (Exception e) {
                Log.I("can not open update.bat:"+e.Message);
                isCheckUpdate = false;
            }
        }

        private string GetNameByUrl(string url)
        {
            string name = "default.exe";
            if (!string.IsNullOrEmpty(url))
            {
                var index = url.LastIndexOf("/");
                if (index >= 0)
                {
                    name = url.Substring(index+1,url.Length-index-1);
                }
            }  
            return name;
        }
    }
}
