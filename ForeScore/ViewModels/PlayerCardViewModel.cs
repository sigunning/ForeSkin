using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using ForeScore.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml.Schema;
using ForeScore.Helpers;

namespace ForeScore.ViewModels
{
    class PlayerCardViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        public PlayerCardViewModel()
        {
            azureService = DependencyService.Get<AzureService>();

        }


        private PlayerScore _playerScore;
        public PlayerScore PlayerScore
        {
            get { return _playerScore; }
            set
            {
                _playerScore = value;
                OnPropertyChanged();
            }
        }

        private Round _round;
        public Round Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
            }
        }

        private Course _course;
        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;
                OnPropertyChanged();
            }
        }

        // player card list
        private List<PlayerCard> _playerCards;
        public List<PlayerCard> PlayerCards
        {
            get { return _playerCards; }
            set
            {
                _playerCards = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // load the lookups and picker lists.
            // get course from RoundId
            Round = await azureService.GetRound(PlayerScore.RoundId);
            Course = await azureService.GetCourse(Round.CourseId);

            // set title
            Title = Course.CourseName + " " + Round.RoundDate.ToString("dd-MM-yy");

            // load up player scorecard
            List<PlayerCard> lstPlayerCards = new List<PlayerCard>();
            lstPlayerCards.Add(GetCardHole(10, 1, "1", Course.H1_Par, Course.H1_SI, PlayerScore.S1));
            lstPlayerCards.Add(GetCardHole(20, 2, "2", Course.H2_Par, Course.H2_SI, PlayerScore.S2));
            lstPlayerCards.Add(GetCardHole(30, 3, "3", Course.H3_Par, Course.H3_SI, PlayerScore.S3));
            lstPlayerCards.Add(GetCardHole(40, 4, "4", Course.H4_Par, Course.H4_SI, PlayerScore.S4));
            lstPlayerCards.Add(GetCardHole(50, 5, "5", Course.H5_Par, Course.H5_SI, PlayerScore.S5));
            lstPlayerCards.Add(GetCardHole(60, 6, "6", Course.H6_Par, Course.H6_SI, PlayerScore.S6));
            lstPlayerCards.Add(GetCardHole(70, 7, "7", Course.H7_Par, Course.H7_SI, PlayerScore.S7));
            lstPlayerCards.Add(GetCardHole(80, 8, "8", Course.H8_Par, Course.H8_SI, PlayerScore.S8));
            lstPlayerCards.Add(GetCardHole(90, 9, "9", Course.H9_Par, Course.H9_SI, PlayerScore.S9));
            lstPlayerCards.Add(GetCardHole(100, 10, "10", Course.H10_Par, Course.H10_SI, PlayerScore.S10));
            lstPlayerCards.Add(GetCardHole(110, 11, "11", Course.H11_Par, Course.H11_SI, PlayerScore.S11));
            lstPlayerCards.Add(GetCardHole(120, 12, "12", Course.H12_Par, Course.H12_SI, PlayerScore.S12));
            lstPlayerCards.Add(GetCardHole(130, 13, "13", Course.H13_Par, Course.H13_SI, PlayerScore.S13));
            lstPlayerCards.Add(GetCardHole(140, 14, "14", Course.H14_Par, Course.H14_SI, PlayerScore.S14));
            lstPlayerCards.Add(GetCardHole(150, 15, "15", Course.H15_Par, Course.H15_SI, PlayerScore.S15));
            lstPlayerCards.Add(GetCardHole(160, 16, "16", Course.H16_Par, Course.H16_SI, PlayerScore.S16));
            lstPlayerCards.Add(GetCardHole(170, 17, "17", Course.H17_Par, Course.H17_SI, PlayerScore.S17));
            lstPlayerCards.Add(GetCardHole(180, 18, "18", Course.H18_Par, Course.H18_SI, PlayerScore.S18));

            // ---------------------
            // now the totals. 
            // ---------------------

            // Front nine
            PlayerCard prcOut = new PlayerCard()
            {
                Seq = 95, 
                HoleNo = 0,
                HoleName = "OUT",
                SI = 0,
                HCAP = 0,
                PlayerId = PlayerScore.PlayerId
            };
            prcOut.Par = lstPlayerCards.Where(o => o.HoleNo <= 9).Sum(o => o.Par);
            prcOut.Score = lstPlayerCards.Where(o => o.HoleNo <= 9).Sum(o => o.Score);
            prcOut.Net = lstPlayerCards.Where(o => o.HoleNo <= 9).Sum(o => o.Net);
            prcOut.Pts = lstPlayerCards.Where(o => o.HoleNo <= 9).Sum(o => o.Pts);
            lstPlayerCards.Add(prcOut);

            // Back nine
            PlayerCard prcIn = new PlayerCard()
            {
                Seq = 185,
                HoleNo = 0,
                HoleName = "IN",
                SI = 0,
                HCAP = 0,
                PlayerId = PlayerScore.PlayerId
            };
            prcIn.Par = lstPlayerCards.Where(o => o.HoleNo >= 10).Sum(o => o.Par);
            prcIn.Score = lstPlayerCards.Where(o => o.HoleNo >= 10).Sum(o => o.Score);
            prcIn.Net = lstPlayerCards.Where(o => o.HoleNo >= 10).Sum(o => o.Net);
            prcIn.Pts = lstPlayerCards.Where(o => o.HoleNo >= 10).Sum(o => o.Pts);
            lstPlayerCards.Add(prcIn);

            // Total Round
            PlayerCard prcTot = new PlayerCard()
            {
                Seq = 200,
                HoleNo = 0,
                HoleName = "TOTAL",
                SI = 0,
                HCAP = 0,
                PlayerId = PlayerScore.PlayerId
            };
            prcTot.Par = prcOut.Par + prcIn.Par;
            prcTot.Score = prcOut.Score + prcIn.Score;
            prcTot.Net = prcOut.Net + prcIn.Net;
            prcTot.Pts = prcOut.Pts + prcIn.Pts;
            lstPlayerCards.Add(prcTot);



            // assign to property. Order by sequence No
            PlayerCards = lstPlayerCards.OrderBy(o => o.Seq).ToList();

            IsBusy = false;
        }

        private PlayerCard GetCardHole(int seq, int holeNo, string holeName, int par, int sI, int score)
        {
            // create a new scorecard hole set
            PlayerCard pc = new PlayerCard()
            {
                Seq = seq,
                HoleNo = holeNo,
                HoleName = holeName,
                Par=par,
                SI = sI,
                Score = score,
                HCAP = PlayerScore.HCAP,
                PlayerId = PlayerScore.PlayerId,
                Net = score == 0 ? 0 : score - Utils.GetShots(PlayerScore.HCAP, sI),
                Pts = score == 0 ? 0 : Utils.GetPoints(score, par, PlayerScore.HCAP, sI)
            };

            return pc;

        }


    }
}
