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

    }
}
