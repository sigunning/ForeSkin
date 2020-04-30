using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class CourseHoles
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int HoleID { get; set; }

        public int CourseID_FK { get; set; }
        public string Course_id { get; set; }

        public int HoleNumber { get; set; }
        public int HolePar { get; set; }
        public int HoleSI { get; set; }
    }
}
