using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using Newtonsoft.Json;

namespace ForeScore.Models
{
    public class Course
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int CourseID { get; set; }

        public string CourseName { get; set; }
        public int PAR { get; set; }
        public int SS { get; set; }
        public int Yards { get; set; }

    }
}
