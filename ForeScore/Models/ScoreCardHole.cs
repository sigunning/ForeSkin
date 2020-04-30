using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class ScoreCardHole
    {
        public int Sequence { get; set; }
        public string PlayerRound_id { get; set; }
        public string Round_id { get; set; }

        public string Player_id { get; set; }
        public string PlayerName { get; set; }

        public string PlayerHcap
        {
            get { return string.Concat(PlayerName, " (", HCAP.ToString(), ")"); }
        }

        public string CourseName { get; set; }

        public int HCAP { get; set; }

        public int Hole { get; set; }

        public string HoleName { get; set; }

        public int HoleSI { get; set; }

        public int HolePar { get; set; }

        public int Gross { get; set; }

        public int Strokes { get; set; }

        public string StrokeStars { get; set; }

        public int Net  { get; set; }

        public int Points { get; set; }

        public int CumPoints { get; set; }

        public string HoleBackColor
        {
            get { return Hole > 0 ? "#2e7d32" : "#c68400"; }
        }
        public string RowBackColor
        {
            get { return Hole > 0 ? "#ffffff" : "#ffe54c"; }
        }
        public string GrossForeColor
        {
            get { return Gross>0 && Gross<HolePar  ? "#FF0000" : "#000000"; }
        }

    }
}
