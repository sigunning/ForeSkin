using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class Round
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int RoundID_FK { get; set; }
        public int TournamentID_FK { get; set; }
        public int CourseID_FK { get; set; }
        public string Course_id { get; set; }
        public string Tournament_id { get; set; }

        public DateTime RoundDate { get; set; }
        public int Format { get; set; }
        public int HcapPct { get; set; }

        
     
    }
}
