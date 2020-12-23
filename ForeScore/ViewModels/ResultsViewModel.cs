using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using ForeScore.Helpers;

namespace ForeScore.ViewModels
{
    class ResultsViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        private string _competitionId;
        

        public ResultsViewModel(string competitionId = null )
        {
            azureService = DependencyService.Get<AzureService>();
            Title = "Results";
            _competitionId = competitionId?? Preferences.Get("CompetitionId", null);
        }


        private ObservableCollection<PlayerSummaryScore> _playerSummaryScores;
        public ObservableCollection<PlayerSummaryScore> PlayerSummaryScores
        {
            get { return _playerSummaryScores; }
            set
            {
                _playerSummaryScores = value;
                OnPropertyChanged();
            }
        }

        private string _courseName;
        public string CourseName
        {
            get { return _courseName; }
            set
            {
                _courseName = value;
                OnPropertyChanged();
            }
        }

        private string _competitionName;
        public string CompetitionName
        {
            get { return _competitionName; }
            set
            {
                _competitionName = value;
                OnPropertyChanged();
            }
        }

        

        // -----------------------------------------------------------------------
        // Round Picker
        // -----------------------------------------------------------------------

        // private backing vars for lists
        private ObservableCollection<Round> _rounds;
        // Main source for the rounds list
        public ObservableCollection<Round> Rounds
        {
            get { return _rounds; }
            set
            {
                _rounds = value;
                OnPropertyChanged();
            }
        }

        // property bound to picker selectedround
        private Round _selectedRound;
        public Round SelectedRound
        {
            get { return _selectedRound; }
            set
            {
                _selectedRound = value;

                // store to settings               
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedRound in Preferences to: " + value.CourseName);
                    Preferences.Set("RoundId", value.RoundId);
                    LoadScores();
                }
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
        private Competition _selectedCompetition;
        public async void LoadData()
        {

            // get data for passed in comp
            if (_competitionId == null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            // load lookups for the Round model 
            await azureService.LoadCourseLookup();
            await azureService.LoadPlayerLookup();
            await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));
            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty));

            // get competition 
             _selectedCompetition= await azureService.GetCompetition(_competitionId);
            CompetitionName = _selectedCompetition.CompetitionName;

            Title = CompetitionName;

            //Load rounds
            Rounds = await azureService.GetRoundsForCompetition(_competitionId);
            Rounds.Insert(0, new Round() );

            
            // get comp 
            if (Rounds.Count > 0)
            {
                SelectedRound = Rounds.FirstOrDefault(o => o.RoundId == Preferences.Get("RoundId", string.Empty).ToString());
                
            }

            IsBusy = false;


        }

        private async Task LoadScores()
        {
            ObservableCollection<PlayerSummaryScore> lstPlayerSummaryScores = new ObservableCollection<PlayerSummaryScore>();

            // all rounds for comp or just selected round?
            foreach (Round round in Rounds)
            {
                // if All rounds is picked, ignore
                if (round.CourseName == "All") continue;
                // include this round?
                if (round == SelectedRound || SelectedRound.CourseName=="All")
                {
                    // get course
                    Course = await azureService.GetCourse(round.CourseId);

                    // get playerscores
                    ObservableCollection<PlayerScore> lstPlayerScores = await azureService.GetPlayerScores(round.RoundId);

                    // create score summaries
                    foreach (PlayerScore ps in lstPlayerScores)
                    {
                        int hcap = ps.HCAP;
                        int out_Score = ps.S1 + ps.S2 + ps.S3 + ps.S4 + ps.S5 + ps.S6 + ps.S7 + ps.S8 + ps.S9;
                        int in_Score = ps.S10 + ps.S11 + ps.S12 + ps.S13 + ps.S14 + ps.S15 + ps.S16 + ps.S17 + ps.S18;
                        int tot_Score = out_Score + in_Score;

                        int out_Net = (ps.S1 - Utils.GetShots(hcap, Course.H1_SI))+
                            (ps.S2 - Utils.GetShots(hcap, Course.H2_SI));
                        int out_Pts = (Utils.GetPoints(ps.S1, Course.H1_Par, hcap, Course.H1_SI)) +
                            (Utils.GetPoints(ps.S2, Course.H2_Par, hcap, Course.H2_SI));

                        // create summary score and add to list
                        PlayerSummaryScore pss = new PlayerSummaryScore();
                        pss.PlayerId = ps.PlayerId;
                        pss.PlayerName = ps.PlayerName;
                        pss.Out_Score = out_Score;
                        pss.In_Score = in_Score;
                        pss.Tot_Score = tot_Score;
                        pss.Out_Net = out_Net;
                        pss.Out_Pts = out_Pts;
                        lstPlayerSummaryScores.Add(pss);
                    }

                }

            }

            PlayerSummaryScores = lstPlayerSummaryScores;
        }



    }
}
