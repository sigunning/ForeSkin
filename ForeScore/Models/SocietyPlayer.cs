using System;
using System.Collections.Generic;
using System.Text;
using ForeScore.Common;

namespace ForeScore.Models
{
    public class SocietyPlayer
    {
        public string Id { get; set; }
        public string SocietyId { get; set; }
        public string PlayerId { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool SocietyAdmin { get; set; }
        public bool HomeYN { get; set; }
        public bool DeletedYN { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string PlayerName
        {
            get { return PlayerId == null ? string.Empty : Lookups._dictPlayers[PlayerId]; }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string SocietyName
        {
            get { return SocietyId == null ? string.Empty : Lookups._dictSocieties[SocietyId]; }
        }
        
    }
}
