using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.utils
{
    public class SharePreference
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static string IniFileName = "FaceSign.ini";
        public static string Section = "FaceSign";

        public static string Read(string Key, string defaultText)
        {
            var file = $@"{FileUtil.GetAppRootPath()}\{IniFileName}";
            if (File.Exists(file))
            {
                FileInfo info = new FileInfo(file);
                int length = (int)(info.Length);
                StringBuilder builder = new StringBuilder();
                GetPrivateProfileString(Section, Key, defaultText, builder, length, file);
                return builder.ToString();
            }
            else
            {
                return defaultText;
            }
        }

        public static long Write(string Key, string value)
        {
            var file = $@"{FileUtil.GetAppRootPath()}\{IniFileName}";
            return WritePrivateProfileString(Section, Key, value, file);
        }

        public static string Read(string section,string Key, string defaultText)
        {
            var file = $@"{FileUtil.GetAppRootPath()}\{IniFileName}";
            if (File.Exists(file))
            {
                FileInfo info = new FileInfo(file);
                int length = (int)(info.Length);
                StringBuilder builder = new StringBuilder();
                GetPrivateProfileString(section, Key, defaultText, builder, length, file);
                return builder.ToString();
            }
            else
            {
                return defaultText;
            }
        }

        public static long Write(string section, string Key, string value)
        {
            var file = $@"{FileUtil.GetAppRootPath()}\{IniFileName}";
            return WritePrivateProfileString(section, Key, value, file);
        }
    }
}
