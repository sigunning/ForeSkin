﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;

namespace ForeScore.ViewModels
{
    class PlayerDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Player _player;
        public bool IsNew;

        public ICommand SaveCommand { private set; get; }

        public PlayerDetailViewModel()
        {
            //Title = Player.PlayerName?? "Add New Player";
            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SaveCommand = new Command(async () =>
            {
                // all valid?
                if (!Validate())
                    return;

                IsBusy = true;
                // save player then inform subscribers. Ensure user is admin if player=user 
                // or may lock yourself out. Only admins can save.
                if (_player.PlayerId == UserPlayer.PlayerId)
                    _player.AdminYN = true;

                await azureService.SavePlayerAsync(_player);
                if (IsNew)
                {
                    MessagingCenter.Send(_player, "AddNew");
                }
                else
                {
                    MessagingCenter.Send(_player, "Update");
                }

                Debug.WriteLine("Player saved: ");
                await Shell.Current.Navigation.PopAsync();
                IsBusy = false;

            });
        }

        public Player Player
        {
            get { return _player; }
            set
            {
                _player = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Society> _societies;
        public ObservableCollection<Society> Societies
        {
            get { return _societies; }
            set
            {
                _societies = value;
                OnPropertyChanged();
            }
        }


        private string _validationText;
        public string ValidationText
        {
            get { return _validationText; }
            set
            {
                _validationText = value;
                OnPropertyChanged(); ;
            }
        }



        private bool Validate()
        {
            bool isValid = (Player != null && _player.PlayerName != null);
            ValidationText = (isValid ? string.Empty : "Please enter player Name");
            return isValid;
        }

        public async Task LoadData()
        {

            // get soc members
            if (!IsNew)
                Societies = await azureService.GetSocieties(Player.PlayerId);
        }
    }


}
