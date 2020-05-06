using FaceSign.data;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http.req
{
    public class InitRequest
    {
        public string hardkey="";
        public string type = "1";//门禁机
        public string model = "";
        public string ip {
            get { return Util.GetIp(); }
            set { ip = value; }
        }
        public string mac{
            set { mac = value; }
            get { return Util.GetMac(); }
        }
        public string os_type = "windows";
        public string sub_type = "10";//高德设备
        public string app_name = BuildConfig.AppName;
        public string sign = "win64";
        public string version_name = BuildConfig.VersionName;
        public string version_code = $@"{BuildConfig.VersionCode}";
    }
}
