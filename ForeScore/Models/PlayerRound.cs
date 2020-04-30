using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class PlayerRound
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int PlayerRoundID { get; set; }
        
        public string Player_id { get; set; }

        public string Round_id { get; set; }

        public int HCAP { get; set; }
        public string Marker_id { get; set; }
    }
}
