using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http.rep
{
    public class InitResponse:Response
    {
        public string qrcode;
        public string terminal_id;
        public string name;
        public string board_type;
        public string password_flag;
        public string password;
    }
}
