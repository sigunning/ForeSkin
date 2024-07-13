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

namespace ForeScore.ViewModels 
{
    class DiscardViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        public ICommand SaveCommand { private set; get; }


        public DiscardViewModel()
        {
            azureService = DependencyService.Get<AzureService>();
            Title = "Discard 9 Holes";

            // Results command
            SaveCommand = new Command(async () =>
            {
                IsBusy = true;
                

                IsBusy = false;

            });

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
                OnPropertyChanged();
                // store to settings and load competitions for this society             
                if (value != null)
                {
                    LoadCompetitionsAsync();
                }

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
                OnPropertyChanged();
                // store to settings               
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedRound in Preferences to: " + value.CourseName);
                    Preferences.Set("RoundId", value.RoundId);

                }


            }
        }

        //------------------------------------------------------------
        // Loading
        //------------------------------------------------------------

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
            SelectedSociety = SocietiesPicker.FirstOrDefault(o => o.SocietyId == Preferences.Get("SocietyId", string.Empty).ToString());

            Debug.WriteLine("Set SelectedSociety in ExecuteLoadSocietiesCommand: " + SelectedSociety.SocietyName);


        }

        // load competitions for society
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

            // set round pref on dropdown
            Debug.WriteLine("Setting SelectedRound in ExecuteLoadRoundsCommand");
            SelectedRound = Rounds.FirstOrDefault(o => o.RoundId == Preferences.Get("RoundId", string.Empty).ToString());


        }


        private async void LoadCompetitionsAsync()
        {
            Debug.WriteLine("Load compets for society " + SelectedSociety.SocietyName);
            await ExecuteLoadCompetitionsCommand();

        }



        

        public async Task LoadData()
        {
            await azureService.LoadCourseLookup();
            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty));

            // load societies picklist
            await ExecuteLoadSocietiesCommand();

        }
    }
}
