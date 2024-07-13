using System;
using ForeScore.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class Competition
    {

        public string Id { get; set; }

        public string CompetitionId { get; set; }

        public string CompetitionName { get; set; }
        public string CompetitionDescription { get; set; }
        public string SocietyId { get; set; }
        public string Venue { get; set; }
        public string Accommodation { get; set; }

        public string CodeName { get; set; }
        public string Winner { get; set; }

        public DateTime StartDate { get; set; }
        public bool ClosedYN { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string SocietyName {
            get { return SocietyId == null ? string.Empty : Lookups._dictSocieties[SocietyId]; }
        }
    }

    public class CompetitionLookup
    {
        public string Id { get; set; }

        public string CompetitionId { get; set; }

        public string CompetitionName { get; set; }
    }
}
