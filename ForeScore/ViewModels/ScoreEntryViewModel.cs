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
using ForeScore.Common;


using System.ComponentModel;

namespace ForeScore.ViewModels 
{
    class ScoreEntryViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        public ICommand SaveScoreCommand { private set; get; }
        public ICommand SavePrevCommand { private set; get; }
        public ICommand SaveNextCommand { private set; get; }
        public ICommand PlayerCardCommand { private set; get; }

        // --------------------------------------------------------
        // picker selections
        // --------------------------------------------------------
        //public ObservableCollection<int> PickerScore { set; get; }

        //private List<int> _holeList;
        public List<int> HoleList { set; get; }
        //{
        //    get { return _holeList; }
        //    set
        //    {
        //        _holeList = value;
        //        OnPropertyChanged();
        //    }
        //}
        

        public ScoreEntryViewModel()
        {
            azureService = DependencyService.Get<AzureService>();

            // populate the hole picker list
            HoleList = Common.Pickers.Picker18.ToList();

            // Score dropdown values
            PickerScore = Pickers.PickerScore;

            SaveScoreCommand = new Command(async () =>
            {
                // save score
                IsBusy = true;
                await SaveScore();
                IsBusy = false;
            });

            SavePrevCommand = new Command(async () =>
            {
                // save score
                IsBusy = true;
                await SaveScore();
                SelectedHole = SelectedHole==1 ? 18 : SelectedHole -1 ;
                IsBusy = false;
            });

            SaveNextCommand = new Command(async () =>
            {
                // save score
                IsBusy = true;
                await SaveScore();
                SelectedHole = SelectedHole==18 ? 1 : SelectedHole + 1;
                IsBusy = false;
            });


            PlayerCardCommand = new Command<PlayerScore>(async (PlayerScore ps)  =>
            {
                // show player scorecard
                IsBusy = true;
                await Shell.Current.Navigation.PushAsync(new PlayerCardPage(ps));
                IsBusy = false;
            });


        }

        private async Task SaveScore()
        {
            // loop though each player and post entered CurrentScore to hole score
            foreach (PlayerScore item in PlayerScores)
            {
                // store currentscore to hole score
                switch (_selectedHole)
                {
                    case 1: item.S1 = item.Current_Score; break;
                    case 2: item.S2 = item.Current_Score; break;
                    case 3: item.S3 = item.Current_Score; break;
                    case 4: item.S4 = item.Current_Score; break;
                    case 5: item.S5 = item.Current_Score; break;
                    case 6: item.S6 = item.Current_Score; break;
                    case 7: item.S7 = item.Current_Score; break;
                    case 8: item.S8 = item.Current_Score; break;
                    case 9: item.S9 = item.Current_Score; break;
                    case 10: item.S10 = item.Current_Score; break;
                    case 11: item.S11 = item.Current_Score; break;
                    case 12: item.S12 = item.Current_Score; break;
                    case 13: item.S13 = item.Current_Score; break;
                    case 14: item.S14 = item.Current_Score; break;
                    case 15: item.S15 = item.Current_Score; break;
                    case 16: item.S16 = item.Current_Score; break;
                    case 17: item.S17 = item.Current_Score; break;
                    case 18: item.S18 = item.Current_Score; break;
             
                    default: break;
                }

                // calc totals
                item.Tot_Score = Utils.GetScoreOut(item) + Utils.GetScoreIn(item);
                item.Tot_Net = Utils.GetNetOut(item, Course) + Utils.GetNetIn(item, Course);
                item.Tot_Pts = Utils.GetPtsOut(item, Course, false) + Utils.GetPtsIn(item, Course, false);

                // save to db
                await azureService.SavePlayerScoreAsync(item);
            }

        }

        private  void SetScoreToCurrent()
        {
            // loop through each score item in list and set the currentScore element to the selected
            // score. this will then appear in the UI slot for editing
            if (SelectedHole > 0)
            {
                foreach (PlayerScore item in PlayerScores)
                {                   
                    switch (SelectedHole)
                    {
                        case 1: SetCurrentHole(item,  item.S1, Course.H1_Par, Course.H1_SI ); break;
                        case 2: SetCurrentHole(item,  item.S2, Course.H2_Par, Course.H2_SI); break;
                        case 3: SetCurrentHole(item, item.S3, Course.H3_Par, Course.H3_SI); break;
                        case 4: SetCurrentHole(item, item.S4, Course.H4_Par, Course.H4_SI); break;
                        case 5: SetCurrentHole(item, item.S5, Course.H5_Par, Course.H5_SI); break;
                        case 6: SetCurrentHole(item, item.S6, Course.H6_Par, Course.H6_SI); break;
                        case 7: SetCurrentHole(item, item.S7, Course.H7_Par, Course.H7_SI); break;
                        case 8: SetCurrentHole(item, item.S8, Course.H8_Par, Course.H8_SI); break;
                        case 9: SetCurrentHole(item, item.S9, Course.H9_Par, Course.H9_SI); break;
                        case 10: SetCurrentHole(item, item.S10, Course.H10_Par, Course.H10_SI); break;
                        case 11: SetCurrentHole(item, item.S11, Course.H1_Par, Course.H1_SI); break;
                        case 12: SetCurrentHole(item, item.S12, Course.H12_Par, Course.H12_SI); break;
                        case 13: SetCurrentHole(item, item.S13, Course.H13_Par, Course.H13_SI); break;
                        case 14: SetCurrentHole(item, item.S14, Course.H14_Par, Course.H14_SI); break;
                        case 15: SetCurrentHole(item, item.S15, Course.H15_Par, Course.H15_SI); break;
                        case 16: SetCurrentHole(item, item.S16, Course.H16_Par, Course.H16_SI); break;
                        case 17: SetCurrentHole(item, item.S17, Course.H17_Par, Course.H17_SI); break;
                        case 18: SetCurrentHole(item, item.S18, Course.H18_Par, Course.H18_SI); break;

            
                        default: break;
                    }

                   
                }
                // store current hole
                Preferences.Set("HoleNumber", SelectedHole);

                // set current par & SI bindable properties
                CurrentPar = Course.arPar[SelectedHole];
                CurrentSI = Course.arSI[SelectedHole];

                
            }
        }


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

        private void SetCurrentHole(PlayerScore ps, int score, int par, int SI)
        {
            // for each player, set the score for this hole
            if (SelectedHole > 0)
            {
                ps.Current_Par = par;
                ps.Current_SI = SI;

                ps.Current_Hole = SelectedHole;

                //ps.Current_Pts = Utils.GetPoints(score, par, ps.HCAP, SI);
                ps.Current_Shots = Utils.GetShots(ps.HCAP, SI, 1);
                ps.Current_Score = score;
            }
        }

        private int _currentPar;
        public int CurrentPar 
        {
            get { return _currentPar; }
            set
            {
                _currentPar = value;
                OnPropertyChanged();
            }
        }

        private int _currentSI;
        public int CurrentSI 
        {
            get { return _currentSI; }
            set
            {
                _currentSI = value;
                OnPropertyChanged();
            }
        }


        private int _selectedHole;
        // bound to HoleList picker
        public int SelectedHole
        {
            get { return _selectedHole; }
            set
            {
                _selectedHole = value;
               
                OnPropertyChanged();
                // when hole changes, set the score slot to the correct hole
                if (value>0)
                    SetScoreToCurrent();

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


        // scores list
        private ObservableCollection<PlayerScore> _playerScores;
        public ObservableCollection<PlayerScore> PlayerScores
        {
            get { return _playerScores; }
            set
            {
                //_playerScores = value;
                //OnPropertyChanged();
                SetProperty(ref _playerScores, value);
                Debug.WriteLine("PlayerScores changed");
            }
        }



        public async Task LoadData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // get players for whom we are marking score
            PlayerScores = await azureService.GetPlayerScores(Round.RoundId, Preferences.Get("PlayerId", null));
            //ObservableCollection<PlayerScore> lstPlayerscores = await azureService.GetPlayerScores(Round.RoundId);
            //PlayerScores = new ObservableCollection<PlayerScore>(lstPlayerscores
            //    .Where(o => o.MarkerId != Preferences.Get("PlayerId", null))
            //    .ToList());



            // get course. This will populate array of Par & SI in course
            Course = await azureService.GetCourse(Round.CourseId);

            

            // go to last hole set
            SelectedHole = Preferences.Get("HoleNumber", 1);

            

            IsBusy = false;
        }

     }
}
