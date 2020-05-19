using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http.req
{
    public class OpenGuardReportRequest
    {
        public string terminal_id="";
        public string person_id="";
        public string person_name = "";
        public string image="";
        public string open_type = "1";
        public string open_time {
            get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { open_time = value; }
        }
        public string open_state = "1";
        public string temp_val = "";
    }
}
