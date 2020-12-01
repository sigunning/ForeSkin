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
using ForeScore.Common;
using System.Security.Cryptography;

namespace ForeScore.ViewModels
{
    class SocietyCompViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;


        // -----------------------------------------------------------------------
        // Button commands
        // -----------------------------------------------------------------------
        public ICommand AddSocietiesCommand { private set; get; }
        public ICommand AddCompetitionsCommand { private set; get; }
        public ICommand AddRoundsCommand { private set; get; }

        public ICommand EditSocietiesCommand { private set; get; }
        public ICommand EditCompetitionsCommand { private set; get; }
        public ICommand EditRoundsCommand { private set; get; }
        public ICommand PlayersCommand { private set; get; }

        //constructor
        public SocietyCompViewModel()
        {
            Title = "Round Setup";

            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            AddSocietiesCommand = new Command(async () =>
            {
                IsBusy = true;
                // create new society object
                Society item = new Society();
                item.SocietyId = Guid.NewGuid().ToString();
                item.CreatedByPlayerId = Preferences.Get("PlayerId", null);
                item.CreatedDate = DateTime.Now;
                await Shell.Current.Navigation.PushAsync(new SocietyDetailPage(item));
                IsBusy = false;

            });

            EditSocietiesCommand = new Command(async () =>
            {
                IsBusy = true;
                // create new society object
                Society item = SelectedSociety;   
                await Shell.Current.Navigation.PushAsync(new SocietyDetailPage(item));
                IsBusy = false;
            });

            AddCompetitionsCommand = new Command(async () =>
            {
                IsBusy = true;
                // create new competition object
                if (SelectedSociety != null)
                {
                    Competition item = new Competition();
                    item.CompetitionId = Guid.NewGuid().ToString();
                    item.SocietyId = SelectedSociety.SocietyId;
                    //item.SocietyName = SelectedSociety.SocietyName;
                    item.StartDate = DateTime.Now;
                    await Shell.Current.Navigation.PushAsync(new CompetitionDetailPage(item));
                }
                IsBusy = false;

            });

            EditCompetitionsCommand = new Command(async () =>
            {
                IsBusy = true;
                if (SelectedCompetition != null)
                {
                    // create new society object
                    Competition item = SelectedCompetition;
                    await Shell.Current.Navigation.PushAsync(new CompetitionDetailPage(item));
                }
                IsBusy = false;
            });

            AddRoundsCommand = new Command(async () =>
            {
                IsBusy = true;
                if (SelectedCompetition != null)
                {
                    // create new round object
                    Round item = new Round();
                    item.RoundId = Guid.NewGuid().ToString();
                    item.CompetitionId = SelectedCompetition.CompetitionId;
                    

                    item.RoundDate = DateTime.Now;
                    await Shell.Current.Navigation.PushAsync(new RoundDetailPage(item));
                }
                IsBusy = false;

            });

            EditRoundsCommand = new Command(async () =>
            {
                IsBusy = true;
                if (SelectedRound != null)
                {
                    // create new round object
                    Round item = SelectedRound;
                    await Shell.Current.Navigation.PushAsync(new RoundDetailPage(item));
                }
                IsBusy = false;

            });

            PlayersCommand = new Command(async () =>
            {
                IsBusy = true;
                // create new competition object
                if (SelectedRound != null)
                {
                   
                    await Shell.Current.Navigation.PushAsync(new CompPlayersPage(SelectedRound));
                }
                IsBusy = false;

            });


        }


        public async Task LoadData()
        {
            await azureService.LoadCourseLookup();
            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty));
            //await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));

            // load courses to lookups
            //if (lstCourses == null)
            //{
            //    Debug.WriteLine("Loading course list...");
            //    lstCourses = await azureService.GetCourses();
            //    Lookups._dictCourses = lstCourses.ToDictionary(x => x.CourseId, x => x.CourseName);
            //    //Pickers.CourseLookups =  lstCourses.Select(x => new CourseLookup { Id = x.Id, CourseId = x.CourseId, CourseName = x.CourseName }).ToList();
            //    //Pickers.CourseLookups = (from x in lstCourses select new CourseLookup { Id=x.Id, CourseId=x.CourseId, CourseName=x.CourseName }).ToList();
            //}

            // load societies picklist
            await ExecuteLoadSocietiesCommand();

        }


        // -----------------------------------------------------------------------
        // Society Picker
        // -----------------------------------------------------------------------
       

        // property bound to picker selecteitem
        private Society _selectedSociety;
        public Society SelectedSociety  
        {
            get { return _selectedSociety; }
            set
            {
                _selectedSociety = value;

                // store to settings and load competitions for this society             
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedSociety property in Preferences to "+value.SocietyName);
                    Preferences.Set("SocietyId", value.SocietyId );
                    LoadCompetitionsAsync();
                    

                }
                OnPropertyChanged();

            }
        }


        // private backing vars for lists
        private List<Society> _societiesPicker;
        // Main source for the societies list
        public List<Society> SocietiesPicker
        {
            get { return _societiesPicker; }
            set
            {
                _societiesPicker = value;
                OnPropertyChanged();
            }
        }

  

        // load societies for current player
        public async Task ExecuteLoadSocietiesCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));
            SocietiesPicker = Common.Pickers.PickerSociety.ToList();

            IsBusy = false;

            // set society pref on dropdown  
            SelectedSociety = SocietiesPicker.FirstOrDefault(o => o.SocietyId == Preferences.Get("SocietyId",string.Empty).ToString() );
           
            Debug.WriteLine("Set SelectedSociety in ExecuteLoadSocietiesCommand: " + SelectedSociety.SocietyName);
           

        }

        // -----------------------------------------------------------------------
        // Competition Picker
        // -----------------------------------------------------------------------

        // private backing vars for lists
        private List<Competition> _competitionsPicker;
        // Main source for the competitions list
        public List<Competition> CompetitionsPicker
        {
            get { return _competitionsPicker; }
            set
            {
                _competitionsPicker = value;
                OnPropertyChanged();
            }
        }

        // property bound to picker selecteitem
        private Competition _selectedCompetition;
        public Competition SelectedCompetition
        {
            get { return _selectedCompetition; }
            set
            {
                _selectedCompetition = value;

                // store to settings               
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedCompetition in Preferences to: "+value.CompetitionName);
                    Preferences.Set("CompetitionId", value.CompetitionId);
                    LoadRoundsAsync();
                }
                OnPropertyChanged();

            }
        }

        private async void LoadCompetitionsAsync()
        {
            Debug.WriteLine("Load compets for society "+ SelectedSociety.SocietyName);
            await ExecuteLoadCompetitionsCommand();
            await ExecuteLoadRoundsCommand();
        }



        // load scompetitions for society
        public async Task ExecuteLoadCompetitionsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            await azureService.LoadCompetitionLookup(SelectedSociety.SocietyId);
            CompetitionsPicker = Common.Pickers.PickerCompetition.ToList();

            IsBusy = false;

            // set competition pref on dropdown
            Debug.WriteLine("Setting SelectedCompetition in ExecuteLoadCompetitionsCommand");
            SelectedCompetition = CompetitionsPicker.FirstOrDefault(o => o.CompetitionId == Preferences.Get("CompetitionId", string.Empty).ToString());
            

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

                }
                OnPropertyChanged();

            }
        }

        private async void LoadRoundsAsync()
        {
            Debug.WriteLine("Load rounds for comp " + SelectedCompetition.CompetitionName);
            await ExecuteLoadRoundsCommand();
        }

        // load rounds for compo
        public async Task ExecuteLoadRoundsCommand()
        {

            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            Rounds = await azureService.GetRoundsForCompetition(SelectedCompetition?.CompetitionId);
            IsBusy = false;

            // set society pref on dropdown
            Debug.WriteLine("Setting SelectedRound in ExecuteLoadRoundsCommand");
            SelectedRound = Rounds.FirstOrDefault(o => o.RoundId == Preferences.Get("RoundId", string.Empty).ToString());
            

        }
    }
}
