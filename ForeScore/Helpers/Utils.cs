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

        public static int GetScoreOut(PlayerScore ps)
        {
            return (ps.S1 + ps.S2 + ps.S3 + ps.S4 + ps.S5 + ps.S6 + ps.S7 + ps.S8 + ps.S9);

        }

        public static int GetScoreIn(PlayerScore ps)
        {
            return (ps.S10 + ps.S11 + ps.S12 + ps.S13 + ps.S14 + ps.S15 + ps.S16 + ps.S17 + ps.S18 );

        }

        public static int GetNetOut(PlayerScore ps, Course course)
        {
            // get strokes, then work out points
            int hcap = ps.HCAP;
            int tot = (ps.S1 - GetShots(hcap, course.H1_SI)) + (ps.S2 - GetShots(hcap, course.H2_SI)) +
                (ps.S3 - GetShots(hcap, course.H3_SI)) + (ps.S4 - GetShots(hcap, course.H4_SI)) +
                (ps.S5 - GetShots(hcap, course.H5_SI)) + (ps.S6 - GetShots(hcap, course.H6_SI)) +
                (ps.S7 - GetShots(hcap, course.H7_SI)) + (ps.S8 - GetShots(hcap, course.H8_SI)) +
                (ps.S9 - GetShots(hcap, course.H9_SI));

            return (tot);
        }

        public static int GetNetIn(PlayerScore ps, Course course)
        {
            // get strokes, then work out points
            int hcap = ps.HCAP;
            int tot = (ps.S10 - GetShots(hcap, course.H10_SI)) + (ps.S11 - GetShots(hcap, course.H11_SI)) + 
                (ps.S12 - GetShots(hcap, course.H12_SI)) +
                (ps.S13 - GetShots(hcap, course.H13_SI)) + (ps.S14 - GetShots(hcap, course.H14_SI)) +
                (ps.S15 - GetShots(hcap, course.H15_SI)) + (ps.S16 - GetShots(hcap, course.H16_SI)) +
                (ps.S17 - GetShots(hcap, course.H17_SI)) + (ps.S18 - GetShots(hcap, course.H18_SI));
               ;

            return (tot);
        }


        public static int GetPtsOut(PlayerScore ps, Course course, bool excludeDiscard)
        {
            // get strokes, then work out points
            int hcap = ps.HCAP;
            int tot = (ps.Discard9 == 1 && excludeDiscard) ? 0 : (GetPoints(ps.S1, course.H1_Par, hcap, course.H1_SI)) + 
            (GetPoints(ps.S2, course.H2_Par, hcap, course.H2_SI)) +
            (GetPoints(ps.S3, course.H3_Par, hcap, course.H3_SI)) + (GetPoints(ps.S4, course.H4_Par, hcap, course.H4_SI)) +
            (GetPoints(ps.S5, course.H5_Par, hcap, course.H5_SI)) + (GetPoints(ps.S6, course.H6_Par, hcap, course.H6_SI)) +
            (GetPoints(ps.S7, course.H7_Par, hcap, course.H7_SI)) + (GetPoints(ps.S8, course.H8_Par, hcap, course.H8_SI)) +
            (GetPoints(ps.S9, course.H9_Par, hcap, course.H9_SI));

            return (tot);
        }

        public static int GetPtsIn(PlayerScore ps, Course course, bool excludeDiscard)
        {
            // get strokes, then work out points
            int hcap = ps.HCAP;
            int tot = (ps.Discard9 == 2 && excludeDiscard) ? 0 : (GetPoints(ps.S10, course.H10_Par, hcap, course.H10_SI)) +
            (GetPoints(ps.S11, course.H11_Par, hcap, course.H11_SI)) +
            (GetPoints(ps.S12, course.H12_Par, hcap, course.H12_SI)) +
            (GetPoints(ps.S13, course.H13_Par, hcap, course.H13_SI)) + (GetPoints(ps.S14, course.H14_Par, hcap, course.H14_SI)) +
            (GetPoints(ps.S15, course.H15_Par, hcap, course.H15_SI)) + (GetPoints(ps.S16, course.H16_Par, hcap, course.H16_SI)) +
            (GetPoints(ps.S17, course.H17_Par, hcap, course.H17_SI)) + (GetPoints(ps.S18, course.H18_Par, hcap, course.H18_SI)) ;

            return (tot);
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
