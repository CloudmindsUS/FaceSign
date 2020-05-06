using FaceSign.db;
using FaceSign.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http.rep
{
    public class PersonsResponse:Response
    {
        public List<string> deleted_data { get; set; }
        public List<PersonModel> updated_data { get; set; }
        public string ts { get; set; }
    }
}
