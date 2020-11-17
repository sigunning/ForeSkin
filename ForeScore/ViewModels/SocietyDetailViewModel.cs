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



        public ICommand SaveCommand { private set; get; }


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
                       await SaveSocietyPlayer(_society);
                       MessagingCenter.Send(_society, "AddNew");
                   }
                   else
                   {
                       MessagingCenter.Send(_society, "Update");
                   }

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


        public async Task ExecuteLoadLookupsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            //DoStuff
            if (Society != null)
            {
                SocietyOwner = await azureService.GetPlayer(Society.CreatedByPlayerId);
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

        public async Task SaveSocietyPlayer(Society society)
        {
            // create society player with current player as admin
            SocietyPlayer societyPlayer = new SocietyPlayer();
            societyPlayer.PlayerId = Preferences.Get("PlayerId", null);
            societyPlayer.SocietyId = society.SocietyId;
            societyPlayer.JoinedDate = DateTime.Now;
            societyPlayer.SocietyAdmin = true;
            // save 
            await azureService.SaveSocietyPlayerAsync(societyPlayer);

        }

    }

    
}
