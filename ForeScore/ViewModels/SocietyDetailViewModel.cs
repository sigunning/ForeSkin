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
using System.Runtime.CompilerServices;

namespace ForeScore.ViewModels
{
    class SocietyDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Society _society;
        public bool IsNew;
        public bool IsNotNew
        { get => !IsNew; }



        public ICommand SaveCommand { private set; get; }
        public ICommand AddPlayerCommand { private set; get; }
        public ICommand RemovePlayerCommand { private set; get; }


        // constructor
        public SocietyDetailViewModel()
        {
            //Title = Course.CourseName?? "Add New Course";
            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SaveCommand = new Command(
                execute: async () =>

               { IsBusy = true;

                   if (!Validate())
                       return;
                   // save record and then notify subscribers 
                   await azureService.SaveSocietyAsync(_society);
                   if (IsNew)
                   {
                       // add current player as a member of society
                       AddSocietyMemberToList(Preferences.Get("PlayerId",null), true);
                       
                       MessagingCenter.Send(_society, "AddNew");
                       IsNew = false;
                   }
                   else
                   {
                       // add members
                       MessagingCenter.Send(_society, "Update");
                   }
                   // add /update members
                   await SaveSocietyPlayers();

                   RefreshCanExecutes();
                   Debug.WriteLine("Society saved: ");
                   await Shell.Current.Navigation.PopAsync();
                   IsBusy = false;
               },
                 canExecute: () =>
                 {
                     return (Society != null) ; //  (Society != null && _society.SocietyName != null && _society.SocietyDescription != null);
                 }
            );

            // implement the ICommands
            AddPlayerCommand = new Command(() =>
            {
                if (SelectedPlayer == null) return;
                if (!Validate()) return;
                IsBusy = true;
                // add player to society
                if (SocietyMembers.FirstOrDefault(x => x.PlayerId == SelectedPlayer.PlayerId) == null)
                {
                    AddSocietyMemberToList(SelectedPlayer.PlayerId, false);
                    SelectedPlayer = null;
                }
                IsBusy = false;

            });

            RemovePlayerCommand = new Command<SocietyPlayer>( (societyPlayer) =>
            {

                IsBusy = true;
                // remove from list, set deleted on and add back to force binding change on list
                SocietyMembers.Remove(societyPlayer);
                societyPlayer.DeletedYN = (!societyPlayer.DeletedYN);
                SocietyMembers.Add(societyPlayer);

                IsBusy = false;

            });

        }

        private void AddSocietyMemberToList(string playerId, bool admin)
        {
            SocietyPlayer item = new SocietyPlayer();
            //item. = Guid.NewGuid().ToString();
            item.PlayerId = playerId;
            item.SocietyId = Society.SocietyId;
            item.SocietyAdmin = admin;
            item.JoinedDate = DateTime.Now;
            SocietyMembers.Add(item);

        }


        private  string _validationText;
        public string ValidationText
        {
            get { return _validationText; }
            set
            {
                _validationText = value;
                OnPropertyChanged();;
            }
        }


        private bool Validate()
        {
            bool isValid = (Society != null && _society.SocietyName != null && _society.SocietyDescription != null);
            ValidationText = (isValid ? string.Empty : "Please enter Society Name and Description");
            return isValid;
        }


        private void RefreshCanExecutes()
        {
            (SaveCommand as Command).ChangeCanExecute();
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


        public async Task LoadData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // load the lookups and picker lists. 
            await azureService.LoadPlayerLookup();
            PlayersPicker = Common.Pickers.PickerPlayer.ToList();

            if (Society != null)
            {
                // get owner of society
                SocietyOwner = await azureService.GetPlayer(Society.CreatedByPlayerId);
                // load members
                SocietyMembers = await azureService.GetSocietyPlayers(Society.SocietyId);
            }


            


            IsBusy = false;
        }

        // player object of society creator
        private Player _societyOwner;
        public Player SocietyOwner 
        {
            get { return _societyOwner; }

            set
            {
                _societyOwner = value;
                OnPropertyChanged();
            }
        }


        


        public Society Society
        {
            get { return _society; }
            set
            {
                _society = value;
                OnPropertyChanged();
                (SaveCommand as Command).ChangeCanExecute();
            }
        }

        public async Task SaveSocietyPlayers()
        {
            foreach (SocietyPlayer item in SocietyMembers)
            {
                if (item.DeletedYN)
                    await azureService.DeleteSocietyPlayerAsync(item);
                else
                    await azureService.SaveSocietyPlayerAsync(item);
            }

        }

        private ObservableCollection<SocietyPlayer> _societyMembers;
        public ObservableCollection<SocietyPlayer> SocietyMembers
        {
            get { return _societyMembers; }
            set
            {
                _societyMembers = value;
                OnPropertyChanged();
            }
        }

    }

    
}
