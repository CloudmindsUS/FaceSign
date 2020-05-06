using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http
{
    public class HttpMethod
    {
        public static string GetPersonList = "pixent-api/service/get_person_list";

        public static string OpenGuardReport = "pixent-api/service/guard/open_guard_report";

        public static string Init = "pixent-api/service/pixsignage2/init";

        public static string Bind = "pixent-api/service/pixsignage2/bind";

        public static string Refresh = "pixent-api/service/pixsignage2/refresh";
    }
}
