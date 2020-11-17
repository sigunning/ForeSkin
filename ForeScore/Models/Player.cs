using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ForeScore.Models
{
    public class Player
    {
        
        public string Id { get; set; }

        public string userid { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public int LastHCAP { get; set; }
        public bool AdminYN { get; set; }
        public bool DeletedYN { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string FullName
        {
            get => string.Concat( FirstName ," " , LastName);
        }
        [Newtonsoft.Json.JsonIgnore]
        public string NameAndHCAP
        {
            get => string.Concat(PlayerName, " (", LastHCAP, ")");
        }
        [Newtonsoft.Json.JsonIgnore]
        public bool RegisteredYN
        {
            get => (userid != null);
        }
        [Newtonsoft.Json.JsonIgnore]
        public string Glyph
        {
            get => (userid == null) ? Helpers.MaterialIcon.PersonOutline : Helpers.MaterialIcon.Person ; 
            //get =>  Helpers.MaterialIcon.PersonOutline;
        }

    }
}
