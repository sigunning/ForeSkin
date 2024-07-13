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
    

    class ReportsViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        public ICommand ResultsCommand { private set; get; }
        public ICommand DiscardsCommand { private set; get; }



        public ReportsViewModel()
        {
            azureService = DependencyService.Get<AzureService>();
            Title = "Reports";

            // Results command
            ResultsCommand = new Command(async () =>
            {
                IsBusy = true;
                if (SelectedCompetition != null)
                {
                    // create new society object
                    Competition item = SelectedCompetition;
                    await Shell.Current.Navigation.PushAsync(new ResultsPage(item));
                }
                IsBusy = false;

            });

            // Results command
            DiscardsCommand = new Command(async () =>
            {
                IsBusy = true;
                if (SelectedCompetition != null)
                {
                    // create new society object
                    Competition item = SelectedCompetition;
                    await Shell.Current.Navigation.PushAsync(new DiscardPage(item));
                }
                IsBusy = false;

            });


        }


        public async Task LoadData()
        {
            IsBusy = false;

            await azureService.LoadCourseLookup();
            // load societies picklist
            await ExecuteLoadSocietiesCommand();

            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty));
           
            

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
                Preferences.Set("SocietyId", value.SocietyId);
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



        // load societies for current player
        public async Task ExecuteLoadSocietiesCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            if (SocietiesPicker == null)
            {
                await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));
                SocietiesPicker = Common.Pickers.PickerSociety.ToList();
            }
            IsBusy = false;

            // set society pref on dropdown  
            SelectedSociety = SocietiesPicker.FirstOrDefault(o => o.SocietyId == Preferences.Get("SocietyId", string.Empty).ToString());

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
                if (value != null)
                {
                    Preferences.Set("CompetitionId", value.CompetitionId);
                }
                _selectedCompetition = value;
                OnPropertyChanged();

            }
        }

        private async void LoadCompetitionsAsync()
        {
            Debug.WriteLine("Load compets for society " + SelectedSociety.SocietyName);
            await ExecuteLoadCompetitionsCommand();
           
        }



        // load scompetitions for society
        public async Task ExecuteLoadCompetitionsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            if (CompetitionsPicker == null)
            {
                await azureService.LoadCompetitionLookup(SelectedSociety.SocietyId);
                CompetitionsPicker = Common.Pickers.PickerCompetition.ToList();
            }

            IsBusy = false;

            // set competition pref on dropdown
            Debug.WriteLine("Setting SelectedCompetition in ExecuteLoadCompetitionsCommand");
            SelectedCompetition = CompetitionsPicker.FirstOrDefault(o => o.CompetitionId == Preferences.Get("CompetitionId", string.Empty).ToString());


        }

        
       
        

        

    }


}
