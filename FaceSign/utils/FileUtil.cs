using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.utils
{
    public class FileUtil
    {
        public static object DirectInfo { get; private set; }

        public static string GetAppRootPath() {
            return AppDomain.CurrentDomain.BaseDirectory;

        }

        public static string GetLogDirPath()
        {
            return $@"{GetAppRootPath()}\Logs";
        }

        public static string GetFaceDirPath()
        {
            var path = $@"{GetAppRootPath()}\face";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            return path;
        }

        public static string GetAvaterDirPath()
        {
            var path = $@"{GetAppRootPath()}\avater";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists) {
                info.Create();
            }
            return path;
        }

        public static string GetUpdatePath()
        {
            var path = $@"{GetAppRootPath()}\update";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            return path;
        }

        public static string GetFaceRecognitionPath()
        {
            var path = $@"{GetAppRootPath()}\recognition";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            return path;
        }

        public static string GetTraffDirPath()
        {
            var path = $@"{GetAppRootPath()}\traff";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            return path;
        }
    }
}
