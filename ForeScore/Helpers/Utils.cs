using ForeScore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeScore.Helpers
{
    public static class Utils
    {

        public static int GetShots(int hCap, int strokeIndex, int HCapPct = 1)
        {
            // 1 = Full; 2 = 7 / 8; 3 = 3 / 4
            switch (HCapPct)
            {
                case 1: return (int)(Math.Floor(1 + (hCap - strokeIndex) / 18.0));
                case 2: return (int)(Math.Floor(1 + (Math.Round(hCap * 7.0 / 8.0, 0) - strokeIndex) / 18.0));
                case 3: return (int)(Math.Floor(1 + (Math.Round(hCap * 3.0 / 4.0, 0) - strokeIndex) / 18.0));
                default: return (int)(Math.Floor(1 + (hCap - strokeIndex) / 18.0));
            }

        }

        public static int GetPoints(int gross, int holePar, int hCap, int strokeIndex, int HCapPct = 1)
        {
            // get strokes, then work out points
            int strokes = GetShots(hCap, strokeIndex, HCapPct);
            return (gross == 0 ? 0 : Math.Max(0, (holePar + strokes - gross + 2)));
        }

        public static int GetTally(PlayerScore ps, int currentHole)
        {
            // get strokes, then work out points
            
            return (0);
        }

        public static PlayerCard GetCardHole( int seq, int holeNo, string holeName,string playerId, int par, int sI, int score, int hcap)
        {
            // create a new scorecard hole set
            PlayerCard pc = new PlayerCard()
            {
                Seq = seq,
                HoleNo = holeNo,
                HoleName = holeName,
                Par = par,
                SI = sI,
                Score = score,
                HCAP = hcap,
                PlayerId = playerId,
                Net = score == 0 ? 0 : score - Utils.GetShots(hcap, sI),
                Pts = score == 0 ? 0 : Utils.GetPoints(score, par, hcap, sI)
            };

            return pc;

        }

    }
}
