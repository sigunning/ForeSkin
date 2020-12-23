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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;


namespace ForeScore.ViewModels
{
    class CompPlayersViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;
        private Round _round;

        // -----------------------------------------------------------------------
        // Button commands
        // -----------------------------------------------------------------------
        public ICommand AddPlayerCommand { private set; get; }
        public ICommand AddMembersCommand { private set; get; }
        public ICommand RemovePlayerCommand { private set; get; }
        public ICommand SavePlayersCommand { private set; get; }

        public CompPlayersViewModel()
        {

            azureService = DependencyService.Get<AzureService>();

            Title = "Players in Round ";

            // implement the ICommands
            AddPlayerCommand = new Command(() =>
           {
               if (SelectedPlayer == null) return;
               IsBusy = true;
                // create new playerscore object, unless exists
                if (PlayerScores.FirstOrDefault(x => x.PlayerId == SelectedPlayer.PlayerId) == null)
               {
                   AddPlayerScoreToList(SelectedPlayer); 
               }
               IsBusy = false;

           });

            RemovePlayerCommand = new Command<PlayerScore>(  (playerScore) =>
            {
    
                IsBusy = true;
                // mark as deleted
                //await azureService.DeletePlayerScoreAsync(playerScore);
                // remove from list, set deleted on and add back to force binding change on list
                PlayerScores.Remove(playerScore);
                playerScore.DeletedYN = (!playerScore.DeletedYN);
                PlayerScores.Add(playerScore);

                IsBusy = false;

            });

            AddMembersCommand = new Command( async ()  =>
           {
               // add all members of society
               IsBusy = true;
               foreach (Player player in PlayersPicker)
               {
                   if (PlayerScores.FirstOrDefault(x => x.PlayerId == player.PlayerId) == null)
                   {
                       AddPlayerScoreToList(player);
                   }
               }

               IsBusy = false;
           });

            SavePlayersCommand = new Command(async () =>
            {
                // save/delete players 
                IsBusy = true;
                foreach (PlayerScore playerScore in PlayerScores)
                {
                    if (playerScore.DeletedYN)
                        await azureService.DeletePlayerScoreAsync(playerScore);
                    else
                        await azureService.SavePlayerScoreAsync(playerScore);
                }

                await Shell.Current.Navigation.PushAsync(new ScoreEntryPage(_round));
                IsBusy = false;
            });
        }


        private void AddPlayerScoreToList(Player player)
        {
            PlayerScore item = new PlayerScore();
            item.PlayerScoreId = Guid.NewGuid().ToString();
            item.MarkerId = Preferences.Get("PlayerId", null);
            item.RoundId = Round.RoundId;
            item.HCAP = player.LastHCAP;
            item.PlayerId = player.PlayerId;
            PlayerScores.Add(item);
            
        }

        public Round Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
                
            }
        }

        private ObservableCollection<PlayerScore> _playerScores;
        public ObservableCollection<PlayerScore> PlayerScores
        {
            get { return _playerScores; }
            set
            {
                _playerScores = value;
                OnPropertyChanged();
            }
        }

        private PlayerScore _selectedPlayerScore;
        public PlayerScore SelectedPlayerScore
        {
            get { return _selectedPlayerScore; }
            set
            {
                _selectedPlayerScore = value;
                OnPropertyChanged();
            }
        }


        private Player _selectedPlayer;
        public Player SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged();
            }
        }

        // private backing vars for lists
        private List<Player> _playersPicker;
        // Main source for the societies list
        public List<Player> PlayersPicker
        {
            get { return _playersPicker; }
            set
            {
                _playersPicker = value;
                OnPropertyChanged();
            }
        }



        public async Task LoadData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // load the lookups and picker lists. Only show members of this society
            await azureService.LoadPlayerLookup();

            // get soc members
            ObservableCollection<SocietyPlayer> societyPlayers = await azureService.GetSocietyPlayers(Round.SocietyId);
    
            // join to player to get player object
            List<Player>  lstPlayersPicker  = new List<Player>();
            PlayersPicker = (from s2 in Common.Pickers.PickerPlayer
                                join s1 in societyPlayers on s2.PlayerId equals s1.PlayerId
                select new Player
                {
                    Id=s1.Id,
                    PlayerId= s1.PlayerId,
                    PlayerName = s2.PlayerName,
                    LastHCAP = s2.LastHCAP
                }).OrderBy(o => o.PlayerName)
                .ToList();

            // show players already in round
            PlayerScores = await azureService.GetPlayerScores(Round.RoundId);

            IsBusy = false;


           
        }

    }

}
