using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class LookupRound
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Course_id { get; set; }

        public DateTime RoundDate { get; set; }

        public string NameAndDate { get; set; }
    }
}
