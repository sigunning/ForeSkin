using System;
using System.Collections.Generic;
using System.Text;

namespace ForeScore.Models
{
    public class Society
    {

        public string Id { get; set; }
        public string SocietyId { get; set; }

        public string SocietyName { get; set; }
        public string SocietyDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByPlayerId { get; set; }

        public bool DeletedYN { get; set; }

        // lookups
        //[Newtonsoft.Json.JsonIgnore]
        //public string SocietyOwner { get; set; }
    }

    public class SocietyLookup
    {
        public string id { get; set; }
        public string SocietyId { get; set; }

        public string SocietyName { get; set; }

    }
}
