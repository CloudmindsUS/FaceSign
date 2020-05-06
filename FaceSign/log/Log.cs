using FaceSign.data;
using FaceSign.utils;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.log
{
    public class Log
    {
        private const string DefaultTag = "FaceSign";

        public static log4net.ILog Logger;

        public static void Close()
        {
            LoggerManager.Shutdown();
        }

        public static void Init()
        {
            ClearLog();
            var filter = new LevelRangeFilter
            {
                LevelMax = Level.Fatal,
                LevelMin = Level.All
            };
            filter.ActivateOptions();

            var layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level - %message%newline");
            var consoleAppender = new ConsoleAppender { Layout = layout };
            var fileAppender = new RollingFileAppender
            {
                AppendToFile = true,
                Encoding = Encoding.UTF8,
                File = FileUtil.GetLogDirPath() + "\\Log",
                LockingModel = new FileAppender.MinimalLock(),
                ImmediateFlush = true,
                Layout = layout,
                DatePattern = "yyyyMMddHH'.txt'",
                CountDirection = 10,
                StaticLogFileName = false,
                MaxSizeRollBackups = 20,
                MaximumFileSize = "1MB",
                RollingStyle = RollingFileAppender.RollingMode.Composite,
            };
            fileAppender.ActivateOptions();
            consoleAppender.ActivateOptions();
            var repository = LoggerManager.CreateRepository("Pixsignage");
            BasicConfigurator.Configure(repository, fileAppender);
            BasicConfigurator.Configure(repository, consoleAppender);
            Logger = log4net.LogManager.GetLogger(repository.Name, "Pixsignage");
            Logger.Info($"Log Start!VersionName:{BuildConfig.VersionName},VersionCode:{BuildConfig.VersionCode}");
        }

        private static void ClearLog()
        {
            var dir = new DirectoryInfo(FileUtil.GetLogDirPath());
            if (!dir.Exists) return;
            var files = dir.GetFiles();
            files = files.OrderByDescending(f => f.CreationTime).ToArray();
            if (files.Length <= 30)
            {
                return;
            }
            for (var i = 30; i < files.Length; i++)
            {
                try
                {
                    files[i].Delete();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void Test()
        {
            while (true)
            {
                I("", "    1.智能茶几桌面浮动效果：最好只让图片浮动，而菜单按钮固定；如果必须要两者同时浮动，也请保证菜单按钮及图片的漂浮范围在屏幕范围内，现在的图片一旦被移动到屏幕边界便无法再移回。 ");
            }
        }

        public static void I(string tag, string msg)
        {
            Logger?.Info($"{tag}:{msg}");
        }

        public static void I(string msg)
        {
            Logger?.Info($"{DefaultTag}:{msg}");
        }

        public static void E(string tag, string msg)
        {
            Logger?.Error($"{tag}:{msg}");
        }

        public static void E(string msg)
        {
            Logger?.Error($"{DefaultTag}:{msg}");
        }

        public static void W(string tag, string msg)
        {
            Logger?.Warn($"{tag}:{msg}");
        }

        public static void W(string msg)
        {
            Logger?.Warn($"{DefaultTag}:{msg}");
        }

        public static void D(string tag, string msg)
        {
            Logger?.Debug($"{tag}:{msg}");
        }

        public static void D(string msg)
        {
            Logger?.Debug($"{DefaultTag}:{msg}");
        }

        public static void Debug(string tag, string msg)
        {
            Logger?.Debug($"{tag}:{msg}");
        }
    }
}
