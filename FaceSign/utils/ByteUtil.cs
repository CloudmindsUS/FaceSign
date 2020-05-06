using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.utils
{
    public class ByteUtil
    {
        public static byte[] StringToBytes(string str)
        {
            if (string.IsNullOrEmpty(str)) return new byte[0];
            byte[] bytes = new byte[str.Length/2];
            for(int i = 0; i < bytes.Length; i++)
            {
                int start = i*2;
                var hex = str.Substring(start,2);
                bytes[i] = Convert.ToByte(hex);
            }
            return bytes;
        }

        public static string BytesToString(byte[] bytes)
        {
            var hex = "";
            if (bytes == null) return hex;
            for (var i = 0; i < bytes.Length; i++) {
                hex += bytes[i].ToString("X2");
            }
            return hex;
        }

        public static byte[] GetCrc(byte[] data)
        {
            var crc= CRC16(data);
            return IntToBytes(crc);
        }

        public static byte[] GetCrc(byte[] data,int len)
        {
            var crc = CRC16(data,len);
            return IntToBytes(crc);
        }

        private static int CRC16(byte[] data)
        {
           return CRC16(data,data.Length);
        }

        public static byte[] IntToBytes(int value) {
            byte[] src = new byte[2];
            src[1] = (byte)((value >> 8) & 0xFF);//高8位
            src[0] = (byte)(value & 0xFF);//低位
            return src;
        }

        private static ushort CRC16(byte[] bytes,int len)
        {
            ushort value;
            ushort newLoad = 0xffff, In_value;
            int count = 0;
            for (int i = 0; i < len; i++)
            {
                value = (ushort)bytes[i];
                newLoad = (ushort)(Convert.ToInt32(value) ^ Convert.ToInt32(newLoad));
                In_value = 0xA001;
                while (count < 8)
                {
                    if (Convert.ToInt32(newLoad) % 2 == 1)//判断最低位是否为1
                    {
                        newLoad -= 0x00001;
                        newLoad = (ushort)(Convert.ToInt32(newLoad) / 2);//右移一位
                        count++;//计数器加一
                        newLoad = (ushort)(Convert.ToInt32(newLoad) ^ Convert.ToInt32(In_value));//异或操作
                    }
                    else
                    {
                        newLoad = (ushort)(Convert.ToInt32(newLoad) / 2);//右移一位
                        count++;//计数器加一
                    }
                }
                count = 0;
            }
            return newLoad;
        }      
    }
}
