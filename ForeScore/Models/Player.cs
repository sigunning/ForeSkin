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

        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public string Password { get; set; }
        public int LastHCAP { get; set; }
        public bool AdminYN { get; set; }
        public bool DeletedYN { get; set; }

    }
}
