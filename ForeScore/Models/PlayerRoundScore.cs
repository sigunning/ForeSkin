using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Models
{
    public class PlayerRoundScore
    {
        public string Id { get; set; }

        public string PlayerRound_id { get; set; }
        public string Round_id { get; set; }

        public string Player_id { get; set; }
        public string PlayerName { get; set; }

        public string PlayerHcap
        {
            get { return string.Concat(PlayerName, " (", HCAP.ToString(),")"); }
        }

        public string CourseName { get; set; }

        public int HCAP { get; set; }
        public int Hole { get; set; }

        public int HoleSI { get; set; }

        public int HolePar { get; set; }

        public int Gross { get; set; }
        public int GrossOriginal { get; set; }

        public int Strokes { get; set; }

        public string StrokeStars { get; set; }

        public int Net {
            get { return Gross == 0 ? 0 : (Gross - Strokes); }
        }

        public int Points
        {
            get { return Gross==0 ? 0 : Math.Max(0, (HolePar + Strokes - Gross + 2)); }
            //get; set;
        }

        public string NetPoints
        {
            get { return Gross == 0 ? "-" : string.Concat( Net.ToString(), "/",Points.ToString() ) ; }
            //set { }
        }

        public int CumGross
        {
            get; set;
        }
        public int CumNet
        {
            get; set;
        }
        public int CumPoints
        {
            get; set;
        }

        public string DisplayPoints
        {
            get; set;
        }
    }
}
