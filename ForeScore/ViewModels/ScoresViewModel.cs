using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.Models;
using ForeScore.Helpers;
using ForeScore.Common;
using ForeScore.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class ScoresViewModel : BaseViewModel
    {
        private INavigation _navigation;
        private AzureService azureService;

        private List<PlayerRoundScore> _playerRoundScores;
        
              
        private string _course_id = Preferences.Get("Course_id", null);
        private ObservableCollection<CourseHoles> _courseHoles;

        // playerRounds set by calling page
        public ObservableCollection<PlayerRound> playerRounds;

        // totals view toggle
        private bool _isTotalView=false;


        // constructor
        public ScoresViewModel(INavigation navigation)
        {
            Title = "Scores";
            _navigation = navigation;
            azureService = DependencyService.Get<AzureService>();

            // Score dropdown values
            PickerScore = Pickers.PickerScore;

            // hole dropdown
            PickerHole = Pickers.Picker18;
        }

        // --------------------------------------------------------
        // picker selections
        // --------------------------------------------------------
        private ObservableCollection<int> pickerScore;
        public ObservableCollection<int> PickerScore
        {
            get { return pickerScore; }
            set
            {
                pickerScore = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<int> pickerHole;
        public ObservableCollection<int> PickerHole
        {
            get { return pickerHole; }
            set
            {
                pickerHole = value;
                OnPropertyChanged();
            }
        }


        // --------------------------------------------------------
        // Properties
        // --------------------------------------------------------
        //
        private string  _faTotals = "\uf1ec";
        public string FAButtonImg_Totals
        {
            get { return _faTotals; }
            set
            {
                _faTotals = value;
                OnPropertyChanged();
            }
        }

        private CourseHoles _currentHole;
        public CourseHoles CurrentHole
        {
            get { return _currentHole; }
            set
            {
                _currentHole = value;
                OnPropertyChanged();
            }
        }

        private int _holeNumber;
        public int HoleNumber
        {
            get { return _holeNumber; }
            set
            {
                _holeNumber = value;
                OnPropertyChanged();
            }
        }

        private int _gotoHoleNumber;
        public int GotoHoleNumber
        {
            get { return _gotoHoleNumber; }
            set
            {
                _gotoHoleNumber = value;
                OnPropertyChanged();
                /*
                if (_gotoHoleNumber > 0)
                {
                    _holeNumber = _gotoHoleNumber;
                    SetCurrentHole();
                }
                */
            }
        }


        private string _tournamentName;
        public string TournamentName 
        {
            get { return _tournamentName; }
            set
            {
                _tournamentName = value;
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


        public List<PlayerRoundScore> PlayerRoundScores
        {
            // all scores for all players in group - needed?
            get { return _playerRoundScores; }
            set
            {
                _playerRoundScores = value;
                OnPropertyChanged();
            }
        }

        public async Task CreatePlayerRoundScores()
        {
            
            // ensure we have score records for each player round
            foreach (PlayerRound pr in playerRounds)
            {
                string playerRound_id = pr.Id;
                               
                List<Scores> scores = await azureService.GetScores(playerRound_id);
                if (scores.Count < 18)
                {
                    Debug.WriteLine(string.Format("Creating scores for player round {0}", playerRound_id));
                    // create score set
                    await azureService.CreateScores(playerRound_id);
                }
            }
        }

        public async Task LoadPlayerRoundScores(bool holeToDate)
        {
            Debug.WriteLine("Start LoadPlayerRoundScores "+ DateTime.Now.Millisecond.ToString());

            // get scores for current hole for all players in group
            List<String> lstPlayerRound_id = playerRounds.Select(o => o.Id).ToList<string>();
            List<Scores> scores = await azureService.GetHoleScores(lstPlayerRound_id, _holeNumber, holeToDate);

            // build score object sfor display and updating
            // ## TODO: Pass HcapPct to GetShots()
            List<PlayerRoundScore> lstPlayerRoundScores = new List<PlayerRoundScore>();
            lstPlayerRoundScores = (from s1 in scores
                                join h1 in _courseHoles on s1.Hole equals h1.HoleNumber
                                join p1 in playerRounds on s1.PlayerRound_id equals p1.Id
                                select new PlayerRoundScore
                                {
                                    Id = s1.Id,
                                    PlayerRound_id = s1.PlayerRound_id,
                                    Hole = s1.Hole,
                                    Gross = s1.Gross,
                                    GrossOriginal = s1.Gross,
                                    HolePar =h1.HolePar,
                                    HoleSI=h1.HoleSI,
                                    HCAP=p1.HCAP,
                                    PlayerName= Lookups._dictPlayers[p1.Player_id], 
                                    Strokes= Utils.GetShots(p1.HCAP, h1.HoleSI),
                                    //Points = Utils.GetPoints(s1.Gross, h1.HolePar, p1.HCAP, h1.HoleSI),
                                    StrokeStars = new String('*', Utils.GetShots(p1.HCAP, h1.HoleSI))
                                })  .OrderBy(o => o.PlayerName)
                                    .ToList();

            
                // calc cumulative points for each player
                foreach (PlayerRoundScore prc in lstPlayerRoundScores)
                {
                    prc.CumPoints = lstPlayerRoundScores.Where(o => o.Hole <= _holeNumber && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Points);
                    prc.DisplayPoints = holeToDate ? prc.CumPoints.ToString() : prc.NetPoints;
   
                }
             
            // set property for binding
            PlayerRoundScores = lstPlayerRoundScores.Where(o => o.Hole == _holeNumber).ToList();

            Debug.WriteLine("End LoadPlayerRoundScores " + DateTime.Now.Millisecond.ToString());
        }

        

        public async Task SetCurrentHole()
        {
            IsBusy = true;

            // set the connection status icon
            this.SetMode();

            // set course hole
            CurrentHole = _courseHoles.FirstOrDefault(o => o.HoleNumber == _holeNumber);

            // reload
            _isTotalView = false;
            await this.LoadPlayerRoundScores(_isTotalView);
            SetTotalsButton();

            // store setting
            Preferences.Set("HoleNumber", HoleNumber) ;

            GotoHoleNumber = 0;

            IsBusy = false;
        }

        public async Task SaveHole()
        {
           
            List<Scores> lstScores;
            lstScores = (from prs1 in PlayerRoundScores.Where(o => o.Gross != o.GrossOriginal)
                            select new Scores
                            {
                                Id = prs1.Id,
                                PlayerRound_id = prs1.PlayerRound_id,
                                Hole = prs1.Hole,
                                Gross = prs1.Gross

                            })
                            .ToList();

            if (lstScores.Count > 0)
            {
                Debug.WriteLine(string.Format("Saving scores for hole {0}", _holeNumber.ToString()));
                await azureService.SaveScores(lstScores);
            }

        }



        public async void OnAppearing()
        {
            base.OnAppearing();

            IsBusy = true;

            // set tournie and course
            TournamentName = Lookups._dictTournaments[Preferences.Get("Tournament_id",null) ];
            CourseName = Preferences.Get("CourseName", null);

            // get course holes
            _courseHoles = await azureService.GetCourseCourseHoles(_course_id);

            // set last hole used
            HoleNumber = Preferences.Get("HoleNumber", 1);

            // create score records, if none exist
            await this.CreatePlayerRoundScores();

            await this.SetCurrentHole();

            IsBusy = false;
        }

        public  async void OnHoleSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (_gotoHoleNumber > 0)
            {
                _holeNumber = _gotoHoleNumber;
                await this.SetCurrentHole();
            }
            
        }

        public Command HelpCommand
        {
            get
            {
                return new Command((e) =>
                {
                    
                    App.Current.MainPage.DisplayAlert("Scoring",
                        string.Concat("Enter gross score for each player. Asterisk (*) indicates strokes received.",
                        "Save to show net score and points.","\r\n","Use arrows to move to next hole or 'Hole' to go to specific hole.",
                        "\r\n","Calculator shows total scores so far."), "Ok");

                });
            }
        }

        public  Command NextHoleCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (IsBusy) return;
                    IsBusy = true;
                    // save hole
                    await this.SaveHole();

                    // next hole
                    HoleNumber++;
                    if (HoleNumber > 18) HoleNumber = 1;
                    await this.SetCurrentHole();
                    IsBusy = false;
                });
            }
        }
        public Command SaveHoleCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (IsBusy) return;
                    IsBusy = true;
                    // save hole
                    await this.SaveHole();
                    await this.LoadPlayerRoundScores(false);
                    
                    IsBusy = false;
                });
            }
        }

        private void SetTotalsButton()
        {
            FAButtonImg_Totals = _isTotalView ? "\uf044" : "\uf1ec";
        }

        public Command TotalsCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (IsBusy) return;
                    IsBusy = true;
                    // toggle view
                    _isTotalView = (!_isTotalView);
                    SetTotalsButton();
                    // save hole
                    await this.SaveHole();
                    await this.LoadPlayerRoundScores(_isTotalView);

                    IsBusy = false;
                });
            }
        }


        public Command PreviousHoleCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (IsBusy) return;
                    IsBusy = true;
                    // save hole
                    await this.SaveHole();

                    // next hole
                    HoleNumber--;
                    if (HoleNumber <1) HoleNumber = 18;
                    await this.SetCurrentHole();
                    IsBusy = false;
                });
            }
        }

        // go to reports page
        public Command ReportsCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsBusy = true;
                //    await _navigation.PushAsync(new ReportsPage());
                    IsBusy = false;
                });
            }
        }

    }
}
