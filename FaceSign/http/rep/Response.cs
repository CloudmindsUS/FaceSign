using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http
{
    public class Response
    {
        public const int SystemError = 1001;
        public const int EmptyHardKey = 1002;
        public const int EmptyTerminalId = 1003;
        public const int InvalidTerminalId = 1004;
        public const int RegisteredTerminalId = 1005;
        public const int TerminalIdNotMatch = 1006;
        public const int LockedTerminalId = 1007;
        public const int NetError = -1;

        public int code;
        public string message;
    }
}
