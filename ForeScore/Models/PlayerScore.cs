using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace ForeScore.Models
{
    public class PlayerScore
    {
        public string Id { get; set; }

        public string PlayerScoreId { get; set; }

        public string PlayerId { get; set; }

        public string RoundId { get; set; }

        public int HCAP { get; set; }
        public string MarkerId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool DeletedYN { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Boolean Mark
        {
            get => (MarkerId == Preferences.Get("PlayerId", null));
        }

        [Newtonsoft.Json.JsonIgnore]
        public string Glyph
        {
           // get => (MarkerId == Preferences.Get("PlayerId",null)) ? Helpers.MaterialIcon.Person : Helpers.MaterialIcon.PersonOutline;
            get =>  Helpers.MaterialIcon.PersonOutline;
        }
    }
}
