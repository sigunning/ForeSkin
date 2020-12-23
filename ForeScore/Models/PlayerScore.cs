using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using ForeScore.Common;
using ForeScore.Helpers;

namespace ForeScore.Models
{
    public class PlayerScore : BaseNotifyModel
    {
        public string Id { get; set; }

        public string PlayerScoreId { get; set; }

        public string PlayerId { get; set; }

        public string RoundId { get; set; }

        public int HCAP { get; set; }
        public string MarkerId { get; set; }

        // Scores
        public int S1 { get; set; }
        public int S2 { get; set; }
        public int S3 { get; set; }
        public int S4 { get; set; }
        public int S5 { get; set; }
        public int S6 { get; set; }
        public int S7 { get; set; }
        public int S8 { get; set; }
        public int S9 { get; set; }
        public int S10 { get; set; }
        public int S11 { get; set; }
        public int S12 { get; set; }
        public int S13 { get; set; }
        public int S14 { get; set; }
        public int S15 { get; set; }
        public int S16 { get; set; }
        public int S17 { get; set; }
        public int S18 { get; set; }




        // Current hole properties - not bound to UI
        [Newtonsoft.Json.JsonIgnore]
        public int Current_Par { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int Current_SI { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int Current_Hole { get; set; }


        // ----------------------------------------------
        // Current Hole slots for Binding to UI
        // These need OnPropertyChanged to inform binding
        // ----------------------------------------------
        private int _current_Score;
        [Newtonsoft.Json.JsonIgnore]
        public int Current_Score
        {
            get { return _current_Score; }
            set
            {
                _current_Score = value;
                // calc points
                Current_Pts = Utils.GetPoints(Current_Score, Current_Par, HCAP, Current_SI);
                OnPropertyChanged();
            }
        }
        private int _current_Pts;
        [Newtonsoft.Json.JsonIgnore]
        public int Current_Pts
        {
            get { return _current_Pts; }
            set
            {
                _current_Pts = value;
                OnPropertyChanged();
            }
        }




        private int _current_Shots;
        [Newtonsoft.Json.JsonIgnore]
        public int Current_Shots
        {
            get { return _current_Shots; }
            set
            {
                _current_Shots = value;
                Current_ShotStars = new String('*', _current_Shots);
                OnPropertyChanged();
            }
        }

        private string _current_ShotStars;
        [Newtonsoft.Json.JsonIgnore]
        public string Current_ShotStars
        {
            get { return _current_ShotStars; }
            set
            {
                _current_ShotStars = value;
                OnPropertyChanged();
            }
        }

        // Is user marking the score of this player? Bound to Switch control
        private bool _mark;
        [Newtonsoft.Json.JsonIgnore]
        public bool Mark
        {
            get
            {
                return (MarkerId == Preferences.Get("PlayerId", null));
            }
            set
            {
                _mark = value;
                MarkerId = _mark ? Preferences.Get("PlayerId", null) : null;
                OnPropertyChanged();
            }
        }



        [Newtonsoft.Json.JsonIgnore]
        public int CurrentHole { get; set; }



        [Newtonsoft.Json.JsonIgnore]
        public bool DeletedYN { get; set; }



        [Newtonsoft.Json.JsonIgnore]
        public string PlayerName
        {
            get => Lookups._dictPlayers[PlayerId].ToString();
        }

        [Newtonsoft.Json.JsonIgnore]
        public string PlayerNameHcap
        {
            get => string.Concat(Lookups._dictPlayers[PlayerId].ToString(), "(", HCAP.ToString(), ")");
        }

        [Newtonsoft.Json.JsonIgnore]
        public string Glyph
        {
            // get => (MarkerId == Preferences.Get("PlayerId",null)) ? Helpers.MaterialIcon.Person : Helpers.MaterialIcon.PersonOutline;
            get => Helpers.MaterialIcon.PersonOutline;
        }



    }

    public class PlayerCard
    {
        public int Seq { get; set; }
        public string PlayerId { get; set; }

        public string RoundId { get; set; }

        public int HCAP { get; set; }

        // scores and pts
        public int HoleNo { get; set; }
        public string HoleName { get; set; }
        public int Par { get; set; }
        public int SI { get; set; }

        public int Score { get; set; }
        public int Net { get; set; }
        //{    get => (Score - Utils.GetShots(HCAP, SI) );        }
        public int Pts { get; set; }
        //{  get => Utils.GetPoints(Score,Par, HCAP, SI);  }

        public bool IsTotal
        {
            get => (!int.TryParse(HoleName, out int number));
        }
        public bool IsUnderPar
        {
            get => (Score == 0 ? false : Score < Par && !IsTotal);
        }

        public bool IsOverPar
        {
            get => (Score == 0 ? false : Score > Par && !IsTotal);
        }

        public bool IsVisible
        {
            get => (Score > 0 && SI > 0);
        }

    }

    public class PlayerSummaryScore
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }

        public int HCAP { get; set; }
        public int Out_Score { get; set; }
        public int In_Score { get; set; }
        public int Tot_Score { get; set; }

        public int Out_Net { get; set; }
        public int In_Net { get; set; }
        public int Tot_Net { get; set; }

        public int Out_Pts { get; set; }
        public int In_Pts { get; set; }
        public int Tot_Pts { get; set; }
    }

}
