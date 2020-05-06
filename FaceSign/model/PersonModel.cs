using SQLite;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.model
{
    public class PersonModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public string person_id { get; set; }
        [SugarColumn(IsNullable =true)]
        public string type { get; set; }
        [SugarColumn(IsNullable = true)]
        public string person_no { get; set; }
        [SugarColumn(IsNullable = true)]
        public string uuid { get; set; }
        [SugarColumn(IsNullable = true)]
        public string gender { get; set; }
        [SugarColumn(IsNullable = true)]
        public string name { get; set; }
        [SugarColumn(IsNullable = true)]
        public string avatar { get; set; }
        [SugarColumn(IsNullable = true)]
        public string face { get; set; }
        [SugarColumn(IsNullable = true)]
        public string rfid { get; set; }
        [SugarColumn(IsNullable = true)]
        public string fingerprint_id { get; set; }
        [SugarColumn(IsNullable = true)]
        public string voice_prompt { get; set; }
        [SugarColumn(IsNullable = true)]
        public string feature { get; set; }
        [SugarColumn(IsIgnore = true)]
        public float temperature { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string RealTimeFace { get; set; }
        [SugarColumn(IsIgnore = true)]
        public float smiliaty { get; set; }

    }
}
