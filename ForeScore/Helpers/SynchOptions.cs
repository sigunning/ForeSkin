using System;
using System.Collections.Generic;
using System.Text;

namespace ForeScore.Helpers
{
    public class SyncOptions
    {
        public bool Scores { get; set; }
        public bool Competitions { get; set; }
        public bool Courses { get; set; }
        public bool Societies { get; set; }
        public bool Players { get; set; }
        public string SyncMsg { get; set; }

        public bool Reset { get; set; }

        public SyncOptions()
        {
            SyncMsg = "Data Synchronisation Status";
        }

    }


}
