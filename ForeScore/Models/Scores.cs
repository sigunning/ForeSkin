using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class Scores
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int ScoreID { get; set; }

        public string PlayerRound_id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int PlayerRoundID_FK { get; set; }

        public int Hole { get; set; }


        public int Gross { get; set; }

        public int Discard { get; set; }
    }
}
