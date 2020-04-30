using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.Models;
using ForeScore.Views;
using ForeScore;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using ForeScore.Common;
using ForeScore.Helpers;
using System.Collections.ObjectModel;
using ForeScore.Controls;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class PlayerRoundViewModel : BaseViewModel
    {
        // service and property vars
        protected AzureService azureService; // used in derived class
        private ObservableCollection<Tournament> _Tournaments;
        private List<LookupRound> _LookupRounds;
        protected ObservableCollection<Round> lstRounds;
        private ObservableCollection<Course> lstCourses;

        private ObservableCollection<PlayerRound> _PlayerRounds;
        private Round _thisRound;
        private INavigation _navigation;

        // for picklist
        private ObservableCollection<Player> _Players;

        // constructor
        public PlayerRoundViewModel(INavigation navigation)
        {
            Title = "Group";
            _navigation = navigation;
            azureService = DependencyService.Get<AzureService>();
            ThisRound = new Round() { Tournament_id = Preferences.Get("Tournament_id", null), Id = Preferences.Get("Round_id", null)  };

            // set last settings for dropdown selections
            ThisRound.Tournament_id = Preferences.Get("Tournament_id", null);
            ThisRound.Id = Preferences.Get("Round_id", null);

            // Stroke Index dropdown values
            PickerHcap = new ObservableCollection<int>();
            for (int i = -5; i <= 36; i++)
            {
                this.PickerHcap.Add(i);
            }

        }
        // --------------------------------------------------------
        // picker selections
        // --------------------------------------------------------
        private ObservableCollection<int> pickerHcap;
        public ObservableCollection<int> PickerHcap
        {
            get { return pickerHcap; }
            set
            {
                pickerHcap = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<Tournament> Tournaments
        {
            get { return _Tournaments; }
            set
            {
                _Tournaments = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PlayerRound> PlayerRounds
        {
            get { return _PlayerRounds; }
            set
            {
                _PlayerRounds = value;
                OnPropertyChanged();
            }
        }

        public List<LookupRound> LookupRounds
        {
            get { return _LookupRounds; }
            set
            {
                _LookupRounds = value;
                OnPropertyChanged();
            }
        }

        // for new player picker list
        public ObservableCollection<Player> Players
        {
            get { return _Players; }
            set
            {
                _Players = value;
                OnPropertyChanged();
            }
        }


        // Round object passed from Tournament edit page

        public Round ThisRound
        {
            get { return _thisRound; }
            set
            {
                _thisRound = value;
                //string tournie = Lookups._dictTournaments[_thisRound.Tournament_id];
                OnPropertyChanged();
            }
        }

        public async Task LoadTournamentsAsync()
        {

            //List<Tournament> lst = await azureService.GetTournaments();
            Tournaments = await azureService.GetTournaments();

            // load  to lookups
            Lookups._dictTournaments = Tournaments.ToDictionary(x => x.Id, x => x.TournamentName);


        }

        public async void OnAppearing()
        {
            if (IsBusy) return;
            IsBusy = true;

            base.OnAppearing();

            if (Tournaments == null)
            {
                Debug.WriteLine("Loading tournament list...");
                Tournaments = await azureService.GetTournaments();
                Lookups._dictTournaments = Tournaments.ToDictionary(x => x.Id, x => x.TournamentName);
            }


            // load courses to lookups
            if (lstCourses == null)
            {
                Debug.WriteLine("Loading course list...");
                lstCourses = await azureService.GetCourses(true);
                Lookups._dictCourses = lstCourses.ToDictionary(x => x.Id, x => x.CourseName);
            }

            // load players to lookups
            if (_Players == null)
            {
                Debug.WriteLine("Loading playername lookup...");
                Players = await azureService.GetPlayers();
                Lookups._dictPlayers = _Players.ToDictionary(x => x.Id, x => x.PlayerName);
            }


            // set round if we can
            if (LookupRounds == null)
            {
                if (Preferences.Get("Tournament_id", null) != null)
                { 
                    // get last selected tournament
                    Debug.WriteLine(string.Concat("Loading rounds for Tournament:", Lookups._dictTournaments[Preferences.Get("Tournament_id", null)]));
                    LoadRounds(ThisRound.Tournament_id);
                    ThisRound.Id = Preferences.Get("Round_id", null);
                }
            }

            IsBusy = false;
        }

        public async void OnDisappearing()
        {
            // store last selections to settings
            //Settings.Tournament_id = ThisRound.Tournament_id;
            //Settings.Round_id = ThisRound.Id;
        }

        public void OnTournamentSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event

            if (IsBusy) return;
            IsBusy = true;

            Debug.WriteLine(string.Concat("Tournament selected:", Lookups._dictTournaments[((Tournament)e.SelectedItem).Id]));
            ThisRound.Tournament_id = ((Tournament)e.SelectedItem).Id;

            // only load rounds if we have selected a new tournie
            if (Preferences.Get("Tournament_id",null) != ThisRound.Tournament_id)
            {
                Preferences.Set("Tournament_id",  ThisRound.Tournament_id);
                // clear round
                PlayerRounds = null;
                ThisRound.Id = null;
                LoadRounds(ThisRound.Tournament_id);
                // start from first hole
                Preferences.Set("HoleNumber", 1);
            }

            IsBusy = false;
        }

        public async void LoadRounds(string tournament_id)
        {
            Debug.WriteLine("Loading round list...");
            lstRounds = await azureService.GetTournamentRounds(tournament_id);
            //LookupRounds = lstRounds.Select(x => new LookupRound() { Id = x.Id, Name = Lookups._dictCourses[x.Course_id] }).ToList();

            // set last stored round 
            // var item = LookupRounds.First(x => x.Id == Settings.Round_id);
            // Debug.WriteLine(string.Concat("Setting round: ", Settings.Round_id));

            // use Linq query to join Rounds to Course to get source for Round dropdown
            LookupRounds = (from c1 in lstRounds
                            join c2 in lstCourses
                            on c1.Course_id equals c2.Id
                            select new LookupRound
                            {
                                Id = c1.Id,
                                Name = c2.CourseName,
                                RoundDate = c1.RoundDate,
                                NameAndDate = c2.CourseName + " (" + c1.RoundDate.ToString("dd/MM/yy") + " )",
                                Course_id = c2.Id
                            }).ToList();

        }

        public async void OnRoundSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event

            if (IsBusy) return;
            IsBusy = true;

            ThisRound.Id = ((LookupRound)e.SelectedItem).Id;
            Debug.WriteLine(string.Concat("Round selected: ", ((LookupRound)e.SelectedItem).Name));

            // store to settings
            Preferences.Set("Round_id", ThisRound.Id) ;
            Preferences.Set("CourseName", ((LookupRound)e.SelectedItem).Name);
            Preferences.Set("Course_id", ((LookupRound)e.SelectedItem).Course_id);

            // show players...

            PlayerRounds = await azureService.GetPlayerRounds(ThisRound.Id);
            Debug.WriteLine(string.Concat("Player count: ", PlayerRounds.Count.ToString()));

            IsBusy = false;

        }

        public async void OnPlayerSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event

            int index = ((BindablePicker)sender).SelectedIndex;

            // if picker reset, do not process
            if (index == -1) return;

            if (IsBusy) return;
            IsBusy = true;
            // create new playerRound object
            PlayerRound newPlayerRound = new PlayerRound()
            {
                Round_id = ThisRound.Id,
                Player_id = ((Player)e.SelectedItem).Id,
                HCAP = ((Player)e.SelectedItem).LastHCAP,
                Marker_id = Preferences.Get("UserId", null)
            };

            Debug.WriteLine(string.Concat("Player selected: ", ((Player)e.SelectedItem).PlayerName));


            // already in list?
            if (!PlayerRounds.Any(x => x.Player_id == newPlayerRound.Player_id))
            {
                //PlayerRounds.Add(newPlayerRound); // does not update GUI
                await azureService.SavePlayerRoundAsync(newPlayerRound);
                PlayerRounds = await azureService.GetPlayerRounds(ThisRound.Id);
            }
            IsBusy = false;

            // reset picker
            //if (index != -1) ((BindablePicker)sender).SelectedIndex=-1; // de-select the row

        }

        public Command PlayerRemoveCommand
        {
            get
            {
                return new Command(async (e) =>
                {
                    var answer = await App.Current.MainPage.DisplayAlert("Remove Player", "This will remove all scores for this player for this round. Are you sure?", "Yes", "No");
                    if (answer)
                    {

                        if (IsBusy) return;
                        IsBusy = true;
                        var playerRound = (e as PlayerRound);
                        // delete logic on item
                        Debug.WriteLine(string.Concat("Player round removed: ", playerRound.Id));
                        await azureService.DeletePlayerRoundAsync(playerRound);
                        // remove from list
                        PlayerRounds.Remove(playerRound);
                        //PlayerRounds = await azureService.GetPlayerRounds(ThisRound.Id);

                        IsBusy = false;
                    }
                });
            }
        }

        public Command HelpCommand
        {
            get
            {
                return new Command((e) =>
                {
                    App.Current.MainPage.DisplayAlert("Select Player Group",
                        string.Concat("Tag all players in your playing group for whom you wish to keep score.",
                        " Changes to only these players will be saved. "), "Ok");

                });
            }
        }


        public Command PlayCommand
        {
            get
            {
                return new Command(async (e) =>
                {

                    if (IsBusy) return;
                    IsBusy = true;

                    // delete logic on item
                    Debug.WriteLine("Off we go with our merry band: ");

                    // save handicaps and marker - only for those user is marking
                    //List<PlayerRound> myGroup = PlayerRounds.Where(p => p.Marker_id == Preferences.Get("UserId",null) ).ToList();
                    ObservableCollection<PlayerRound> myGroup = new ObservableCollection<PlayerRound>(PlayerRounds.Where(p => p.Marker_id == Preferences.Get("UserId", null)));

                   
                    //foreach (PlayerRound p in myGroup)
                    //{
                    //    Debug.WriteLine(Lookups._dictPlayers[p.Player_id]);
                    //}

                    await azureService.SaveAllPlayerRoundAsync(myGroup);

                    // on to scoring page
                    await _navigation.PushAsync(new ScoresPage(myGroup));

                    IsBusy = false;

                });
            }
        }


    }
}
