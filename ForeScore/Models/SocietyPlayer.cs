using System;
using System.Collections.Generic;
using System.Text;

namespace ForeScore.Models
{
    public class SocietyPlayer
    {
        public string Id { get; set; }
        public string SocietyId { get; set; }
        public string PlayerId { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool SocietyAdmin { get; set; }
        public bool DeletedYN { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string PlayerName { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string SocietyName { get; set; }
    }
}
