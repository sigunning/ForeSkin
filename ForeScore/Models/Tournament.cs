using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class Tournament
    {
        public string Id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int TournamentID { get; set; }


        public string TournamentName { get; set; }
        public string Venue { get; set; }

        public DateTime StartDate { get; set; }
        public bool ClosedYN { get; set; }

    }
}
