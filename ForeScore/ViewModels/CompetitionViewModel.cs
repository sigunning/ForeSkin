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

namespace ForeScore.ViewModels
{
    class CompetitionViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        // -----------------------------------------------------------------------
        // Button commands
        // -----------------------------------------------------------------------

        public ICommand AddCompetitionsCommand { private set; get; }

        public ICommand SelectionCommand { private set; get; }

        //constructor
        public CompetitionViewModel()
        {

            azureService = DependencyService.Get<AzureService>();

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

            SelectionCommand = new Command<Competition>(async (item) =>
            {
                if (item == null)
                {
                    return;
                }

                IsBusy = true;
                Debug.WriteLine(string.Concat("Competition selected is: ", item.CompetitionName));
                await Shell.Current.Navigation.PushAsync(new CompetitionDetailPage(item));
                // un-select the list
                SelectedCompetition = null;
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

                // store to settings and load competitions for this society             
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedSociety property in Preferences to " + value.SocietyName);
                    Preferences.Set("SocietyId", value.SocietyId);
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


        // List of societies
        private ObservableCollection<Competition> _competitions;
        public ObservableCollection<Competition> Competitions
        {
            get { return _competitions; }
            set
            {
                _competitions = value;
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
                // store to settings               
                if (value != null)
                {
                    Debug.WriteLine("Setting SelectedCompetition in Preferences to: " + value.CompetitionName);
                    Preferences.Set("CompetitionId", value.CompetitionId);
            
                }


            }
        }

        public async Task LoadData()
        {
            await ExecuteLoadSocietiesCommand();
            await ExecuteLoadCompetitionsCommand();
        }

        private async void LoadCompetitionsAsync()
        {
            Debug.WriteLine("Load compets for society " + SelectedSociety.SocietyName);
            await ExecuteLoadCompetitionsCommand();
        }


        // load societies for current player
        public async Task ExecuteLoadSocietiesCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            await azureService.LoadSocietyLookup(Preferences.Get("PlayerId", string.Empty));


            IsBusy = false;

            if (Common.Pickers.PickerSociety.Count > 0)
            {
                SocietiesPicker = Common.Pickers.PickerSociety.ToList();
                // set society pref on dropdown  
                string societyId = Preferences.Get("SocietyId", null);
                if (societyId != null)
                {
                    var society = SocietiesPicker.FirstOrDefault(o => o.SocietyId == societyId);
                    if (society != null)
                    {
                        SelectedSociety = society;
                        Debug.WriteLine("Set SelectedSociety in ExecuteLoadSocietiesCommand: " + SelectedSociety.SocietyName);
                    }
                }
            }
        }

        // load scompetitions for society
        public async Task ExecuteLoadCompetitionsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            Competitions = await azureService.GetCompetitionsForSociety(SelectedSociety.SocietyId);

            Title = $"Competitions ({Competitions.Count})";

            IsBusy = false;
        }
    }


}
