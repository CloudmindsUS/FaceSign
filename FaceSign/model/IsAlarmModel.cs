using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.model
{
    public class IsAlarmEventModel
    {
        public string visibleimg;
        public string infraredimg;
        public string alerttime;
        public string DeviceAddr;
        public List<IsAlarmPointModel> AlarmPointList;
    }

    public class IsAlarmPointModel
    {
        public int Type;
        public float temperature;
        public int X;
        public int Y;
        public string faceimg;
    }
}
