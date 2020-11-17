using ForeScore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ForeScore.Models
{
    public class Round
    {
        public string Id { get; set; }

       
        public string RoundId { get; set; }
        public string CompetitionId { get; set; }
        
        public string CourseId { get; set; }

        public DateTime RoundDate { get; set; }
        public int Format { get; set; }
        public int HcapPct { get; set; }

        

        [Newtonsoft.Json.JsonIgnore]
        public string CourseName 
        {
            get { return CourseId == null ? string.Empty : Lookups._dictCourses[CourseId];  }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string CompetitionName
        {
            get { return CompetitionId==null ? string.Empty : Lookups._dictCompetitions[CompetitionId]; }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string SocietyId
        {
            get { return Preferences.Get("SocietyId", string.Empty); }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string SocietyName
        {
            get { return SocietyId == null ? string.Empty : Lookups._dictSocieties[SocietyId]; }
        }

        [Newtonsoft.Json.JsonIgnore]
        public string Tournament_id { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string Course_id { get; set; }

    }
}
