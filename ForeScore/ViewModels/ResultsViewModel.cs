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

        public ICommand PlayerCardCommand { private set; get; }
        public ICommand DiscardCommand { private set; get; }
        public ICommand SaveDiscardCommand { private set; get; }

        private bool _dataLoaded;

        public ResultsViewModel( )
        {
            azureService = DependencyService.Get<AzureService>();
            Title = "No Competition Selected";

            // get the discard exclusion setting
            ExcludeDiscard = Preferences.Get("ExcludeDiscard", false);


            PlayerCardCommand = new Command<PlayerSummaryScore>(async (PlayerSummaryScore ss) =>
            {
                if (SelectedRound.RoundId == "ALL") return;
                // show player scorecard
                IsBusy = true;
                PlayerScore ps = await azureService.GetPlayerScore(ss.RoundId, ss.PlayerId);
                await Shell.Current.Navigation.PushAsync(new PlayerCardPage(ps));
                IsBusy = false;
            });

            DiscardCommand = new Command<PlayerSummaryScore>(async (PlayerSummaryScore ss) =>
            {
                if (SelectedRound.RoundId == "ALL") return;
                // rotate discard status value
                IsBusy = true;
                PlayerScore ps = await azureService.GetPlayerScore(ss.RoundId, ss.PlayerId);
                ss.Discard9++;
                if (ss.Discard9 >= 3)
                    ss.Discard9 = 0;
                ps.Discard9 = ss.Discard9;
                
                IsBusy = false;
            });

            SaveDiscardCommand = new Command (async () =>
            {
                if (SelectedRound.RoundId == "ALL") return;
                // save discard status
                IsBusy = true;
                // loop through and save Discard9 status
                ObservableCollection<PlayerScore> lstPlayerScores = await azureService.GetPlayerScores(SelectedRound.RoundId);
                foreach (PlayerSummaryScore item in PlayerSummaryScores)
                {
                    // find playerscore and update if different
                    PlayerScore ps = lstPlayerScores.Where(o => o.PlayerScoreId == item.PlayerScoreId).FirstOrDefault();
                    if (ps != null && ps.Discard9 != item.Discard9)
                    {
                        ps.Discard9 = item.Discard9;
                        await azureService.SavePlayerScoreAsync(ps);
                    }
                }
                IsBusy = false;
            });
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

        private int _coursePar;
        public int CoursePar
        {
            get { return _coursePar; }
            set
            {
                _coursePar = value;
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

        private Competition _competition;
        public Competition Competition
        {
            get { return _competition; }
            set
            {
                _competition = value;
                OnPropertyChanged();
            }
        }

        private bool _discardMode;
        public bool DiscardMode
        {
            get { return _discardMode; }
            set
            {
                _discardMode = value;
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
                if (_selectedRound != value)
                {
                    _selectedRound = value;
                    OnPropertyChanged();

                    // store to settings               
                    if (value != null)
                    {
                        Debug.WriteLine("Setting SelectedRound in Preferences to: " + value.CourseName);
                        if (value.RoundId != "ALL")
                            Preferences.Set("RoundId", value.RoundId);
                         
                        if (value.RoundId != null)
                            LoadScores();

                    }

                   
                }
            }
        }

        private bool _singleRound;
        public bool SingleRound
        {
            get { return _singleRound; }
            set
            {
                _singleRound = value;
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


        private Boolean _excludeDiscard;
        public Boolean ExcludeDiscard
        {
            get { return _excludeDiscard; }
            set
            {
                _excludeDiscard = value;
                OnPropertyChanged();
                Preferences.Set("ExcludeDiscard", value);
                LoadScores();

            }
        }

        //---------------------------------------------------------------------
        // LOAD DATA ON START
        //---------------------------------------------------------------------
        public async void LoadData()
        {
            // get data for passed in comp
            if (Competition == null)
                return;

            _dataLoaded = true;
            await LoadDataAsync();

           
        }

        public async Task LoadDataAsync()
        {

            if (IsBusy)
                return;

            IsBusy = true;

            // load lookups for the Round model 
            await azureService.LoadCourseLookup();
            await azureService.LoadPlayerLookup();
            await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));
            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty));

            // get competition 
            // _selectedCompetition = await azureService.GetCompetition(_competitionId);
           // CompetitionName = Competition.CompetitionName;

            Title = Competition.CompetitionName;

            //Load rounds
            Rounds = await azureService.GetRoundsForCompetition(Competition.CompetitionId);
            // all overall selector
            Rounds.Insert(0, new Round() {RoundId="ALL",  RoundDate= Competition.StartDate } );

            
            // set round in picker 
            if (Rounds.Count > 1)
            {
                // if saved round in in list, select it, otherwise select first one
                Round prefRound = Rounds.FirstOrDefault(o => o.RoundId == Preferences.Get("RoundId", null));
                if (prefRound != null)
                {
                    SelectedRound = prefRound;
                }
                else
                {
                    SelectedRound = Rounds.FirstOrDefault();
                }
                
            }

            IsBusy = false;


        }

        private void LoadScores()
        {
            Debug.WriteLine("Running LoadScores");
            if (_dataLoaded)
                LoadScoresAsync();
        }


        private async Task LoadScoresAsync()
        {
            IsBusy = true;

            ObservableCollection<PlayerSummaryScore> lstPlayerSummaryScores = new ObservableCollection<PlayerSummaryScore>();

            // reset Par total
            CoursePar = 0;
            int out_Sum =0;
            int in_Sum = 0;

            // all rounds for comp or just selected round?
            foreach (Round round in Rounds)
            {
                // if All rounds item , ignore
                if (round.CourseId == null) continue;
                // include this round?
                if (round.RoundId == SelectedRound.RoundId || SelectedRound.CourseId == null)
                {
                    // get course
                    Course = await azureService.GetCourse(round.CourseId);

                    // get playerscores
                    ObservableCollection<PlayerScore> lstPlayerScores = await azureService.GetPlayerScores(round.RoundId);

                    // PAR
                    out_Sum = Course.H1_Par + Course.H2_Par + Course.H3_Par + Course.H4_Par + Course.H5_Par + Course.H6_Par +
                            Course.H7_Par + Course.H8_Par + Course.H9_Par;
                    in_Sum = Course.H10_Par + Course.H11_Par + Course.H12_Par + Course.H13_Par + Course.H14_Par + Course.H15_Par +
                        Course.H16_Par + Course.H17_Par + Course.H18_Par;

                    // refresh Par total
                    CoursePar += (out_Sum + in_Sum);


                    Debug.WriteLine("Calc summary scores for: " + round.CourseName + " Par:"+ CoursePar.ToString());

                   

                    // create score summaries
                    foreach (PlayerScore ps in lstPlayerScores)
                    {

                        // create summary score and add to list
                        //PlayerSummaryScore pss = new PlayerSummaryScore();
                        PlayerSummaryScore ss = lstPlayerSummaryScores.FirstOrDefault(o => o.PlayerId == ps.PlayerId);
                        // if null, add to list 
                        if (ss == null)
                        {
                            ss = new PlayerSummaryScore();
                            ss.PlayerScoreId = ps.PlayerScoreId;
                            ss.PlayerId = ps.PlayerId;
                            ss.PlayerName = ps.PlayerName;
                            ss.HCAP = ps.HCAP;
                            ss.RoundId = round.RoundId;
                            ss.Discard9 = ps.Discard9;
                            lstPlayerSummaryScores.Add(ss);
                        }

                        // Score
                        ss.Out_Score += Utils.GetScoreOut(ps);
                        ss.In_Score += Utils.GetScoreIn(ps);
                        ss.Tot_Score += (ss.Out_Score + ss.In_Score);


                        // NET
                        
                        int hcap = ps.HCAP;
                        /*
                        out_Sum = (ps.S1 - Utils.GetShots(hcap, Course.H1_SI)) + (ps.S2 - Utils.GetShots(hcap, Course.H2_SI)) +
                            (ps.S3 - Utils.GetShots(hcap, Course.H3_SI)) + (ps.S4 - Utils.GetShots(hcap, Course.H4_SI)) +
                            (ps.S5 - Utils.GetShots(hcap, Course.H5_SI)) + (ps.S6 - Utils.GetShots(hcap, Course.H6_SI)) +
                            (ps.S7 - Utils.GetShots(hcap, Course.H7_SI)) + (ps.S8 - Utils.GetShots(hcap, Course.H8_SI)) +
                            (ps.S9 - Utils.GetShots(hcap, Course.H9_SI));
                        
                        in_Sum =  (ps.S10 - Utils.GetShots(hcap, Course.H10_SI)) +
                            (ps.S11 - Utils.GetShots(hcap, Course.H11_SI)) + (ps.S12 - Utils.GetShots(hcap, Course.H12_SI)) +
                            (ps.S13 - Utils.GetShots(hcap, Course.H13_SI)) + (ps.S14 - Utils.GetShots(hcap, Course.H14_SI)) +
                            (ps.S15 - Utils.GetShots(hcap, Course.H15_SI)) + (ps.S16 - Utils.GetShots(hcap, Course.H16_SI)) +
                            (ps.S17 - Utils.GetShots(hcap, Course.H17_SI)) + (ps.S18 - Utils.GetShots(hcap, Course.H18_SI));
                      
                        ss.Out_Net += out_Sum;
                        ss.In_Net += in_Sum;
                        */

                        ss.Out_Net += Utils.GetNetOut(ps, Course);
                        ss.In_Net += Utils.GetNetIn(ps, Course);
                        ss.Tot_Net += ss.Out_Net + ss.In_Net;

                        //PTS - take account of Discard flag
                        /*
                        out_Sum = (ps.Discard9 == 1 && ExcludeDiscard) ? 0 : (Utils.GetPoints(ps.S1, Course.H1_Par, hcap, Course.H1_SI)) + (Utils.GetPoints(ps.S2, Course.H2_Par, hcap, Course.H2_SI)) +
                            (Utils.GetPoints(ps.S3, Course.H3_Par, hcap, Course.H3_SI)) + (Utils.GetPoints(ps.S4, Course.H4_Par, hcap, Course.H4_SI)) +
                            (Utils.GetPoints(ps.S5, Course.H5_Par, hcap, Course.H5_SI)) + (Utils.GetPoints(ps.S6, Course.H6_Par, hcap, Course.H6_SI)) +
                            (Utils.GetPoints(ps.S7, Course.H7_Par, hcap, Course.H7_SI)) + (Utils.GetPoints(ps.S8, Course.H8_Par, hcap, Course.H8_SI)) +
                            (Utils.GetPoints(ps.S9, Course.H9_Par, hcap, Course.H9_SI));
                        in_Sum = (ps.Discard9 == 2 && ExcludeDiscard) ? 0 : (Utils.GetPoints(ps.S10, Course.H10_Par, hcap, Course.H10_SI)) +
                            (Utils.GetPoints(ps.S11, Course.H11_Par, hcap, Course.H11_SI)) + (Utils.GetPoints(ps.S12, Course.H12_Par, hcap, Course.H12_SI)) +
                            (Utils.GetPoints(ps.S13, Course.H13_Par, hcap, Course.H13_SI)) + (Utils.GetPoints(ps.S14, Course.H14_Par, hcap, Course.H14_SI)) +
                            (Utils.GetPoints(ps.S15, Course.H15_Par, hcap, Course.H15_SI)) + (Utils.GetPoints(ps.S16, Course.H16_Par, hcap, Course.H16_SI)) +
                            (Utils.GetPoints(ps.S17, Course.H17_Par, hcap, Course.H17_SI)) + (Utils.GetPoints(ps.S18, Course.H18_Par, hcap, Course.H18_SI));

                        ss.Out_Pts +=   out_Sum;
                        ss.In_Pts +=  in_Sum;
                        */

                        ss.Out_Pts += Utils.GetPtsOut(ps, Course,ExcludeDiscard);
                        ss.In_Pts += Utils.GetPtsIn(ps, Course, ExcludeDiscard);
                        ss.Tot_Pts += (ss.Out_Pts + ss.In_Pts);


                    }

                    


                }

            }
            

            // assign sorted list to binded collection
            PlayerSummaryScores = new ObservableCollection<PlayerSummaryScore>( lstPlayerSummaryScores.OrderBy(o => o.Tot_Pts*-1) );
            Debug.WriteLine("Updates Player Summary scores: " + PlayerSummaryScores.Count.ToString() );

            IsBusy = false;

            // set single round to enable/disable buttons
            SingleRound = (SelectedRound.RoundId != "ALL");

            
        }



    }
}
