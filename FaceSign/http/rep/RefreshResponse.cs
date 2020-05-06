using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http.rep
{
    public class RefreshResponse:Response
    {
        public string version_name;
        public int version_code;
        public string url;
        public string md5;
    }
}
