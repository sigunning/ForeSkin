using ForeScore.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

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

        [Newtonsoft.Json.JsonIgnore]
        public string Glyph
        {
            get => (SocietyId == StaticHelpers.UserPlayer.HomeSocietyId ? Helpers.MaterialIcon.People : Helpers.MaterialIcon.PeopleOutline );
            //get =>  Helpers.MaterialIcon.PersonOutline;
        }

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
