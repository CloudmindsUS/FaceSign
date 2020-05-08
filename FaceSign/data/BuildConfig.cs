using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.data
{
    public class BuildConfig
    {
        public const string ChannelG120 = "ChannelG120";
        public const string ChannelG120_AI = "ChannelG120_AI";
        public const string ChannelXT236 = "ChannelXT236";
        public const string ChannelXT236_AI = "ChannelXT236_AI";
        public const string ChannelM120 = "ChannelM120";
        public const string ChannelM120_AI = "ChannelM120_AI";
        public const string Channel_DT_Hospital = "Channel_DT_Hospital";
        public const string Channel_JL = "Channel_JL";
        public const string Channel_America = "Channel_America";

        public const string IR_G120 = "IR_G120";
        public const string IR_XT236 = "IR_XT236";
        public const string IR_M120 = "IR_M120";

        public const string VersionName = "2.0.5";
        public const int VersionCode = 205;
        public static bool Debug = false;

        public static string AppName = "";
        public static string ChannelType = Channel_America;
        public static string IRType = IR_G120;
        //是否支持人脸识别
        public static bool IsSupportAI = false;
        //是否支持金属检测（北京地坛医院专用）
        public static bool IsSupportMetalDetection = false;
        //是否支持门禁
        public static bool IsSupportAccessControl = false;
        //是否支持人脸识别黑名单
        public static bool IsSupportBlacklist = false;
        //是否支持人流量统计
        public static bool IsSupportTrafficStatistics = false;
        //是否支持网络上报
        public static bool IsSupportUploadData = false;


        public static void Init() {
            if (BuildConfig.ChannelType.Equals(ChannelG120))
            {
                InitG120();
            }
            else if (BuildConfig.ChannelType.Equals(ChannelG120_AI))
            {
                InitG120AI();
            }
            else if (BuildConfig.ChannelType.Equals(ChannelXT236))
            {
                InitXT236();
            }
            else if (BuildConfig.ChannelType.Equals(ChannelXT236_AI))
            {
                InitXT236AI();
            }
            else if (BuildConfig.ChannelType.Equals(ChannelM120))
            {
                InitM120();
            }
            else if (BuildConfig.ChannelType.Equals(ChannelM120_AI))
            {
                InitM120AI();
            }
            else if (BuildConfig.ChannelType.Equals(Channel_DT_Hospital))
            {
                InitDT_Hospital();
            }
            else if (BuildConfig.ChannelType.Equals(Channel_JL))
            {
                InitJL();
            }
            else if (BuildConfig.ChannelType.Equals(Channel_America))
            {
                InitAmerica();
            }

        }

        private static void InitAmerica()
        {
            AppName = "FaceSign_America";
            IRType = IR_G120;
            IsSupportUploadData = true;
        }

        private static void InitJL()
        {
            AppName = "FaceSign_JL";
            IRType = IR_M120;
            IsSupportAI = true;
            IsSupportTrafficStatistics = true;
            IsSupportUploadData = true;
        }

        private static void InitDT_Hospital()
        {
            AppName = "FaceSign_DT_Hospital";
            IRType = IR_G120;
            IsSupportUploadData = true;
            IsSupportAI = true;
            IsSupportMetalDetection = true;
            IsSupportAccessControl = true;
            IsSupportBlacklist = true;

        }

        private static void InitM120AI()
        {
            AppName = "FaceSign_M120_AI";
            IRType = IR_M120;
            IsSupportUploadData = true;
            IsSupportAI = true;
        }

        private static void InitM120()
        {
            AppName = "FaceSign_M120";
            IRType = IR_M120;
        }

        private static void InitXT236AI()
        {
            IRType = IR_XT236;
            IsSupportAI = true;
            IsSupportUploadData = true;
            AppName = "FaceSign_XT236_AI";
        }

        private static void InitXT236()
        {
            IRType = IR_XT236;
            IsSupportAI = false;
            AppName = "FaceSign_XT236";
        }

        private static void InitG120AI()
        {
            IRType = IR_G120;
            IsSupportAI = true;
            IsSupportUploadData = true;
            AppName = "FaceSign_G120_AI";
        }

        private static void InitG120()
        {
            IRType = IR_G120;
            IsSupportAI = false;
            AppName = "FaceSign_G120";
        }

        

        public static string GetIRExePath()
        {
            var path = $@"start_ir_g120.bat";
            if (BuildConfig.IR_XT236.Equals(BuildConfig.IRType))
            {
                path = $@"start_ir_xt236.bat";
            }
            else if (BuildConfig.IR_M120.Equals(BuildConfig.IRType)) {
                path = $@"start_ir_m120.bat";
            }
            return $@"{path}";
        }
    }
}
