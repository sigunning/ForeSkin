using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ForeScore.Models;
using Xamarin.Forms;

namespace ForeScore.ViewModels
{
    class PlayerViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        // backing var
        private ObservableCollection<Player> _Players;
        public Command LoadItemsCommand { get; }



        // constructor
        public PlayerViewModel()
        {
            Title = "Players";
            azureService = DependencyService.Get<AzureService>();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        public ObservableCollection<Player> Players
        {
            get { return _Players; }
            set
            {
                _Players = value;
                OnPropertyChanged();
            }
        }

        //public async Task LoadPlayersAsync()
        //{
        //    if (IsBusy) return;
        //    IsBusy = true;
        //    Players = await azureService.GetPlayers();
        //    Title = $"Courses ({Players.Count})";
        //    IsBusy = false;
        //}

        //public async void OnDisplay()
        //{
        //    base.OnAppearing();
        //    Debug.WriteLine("Loading players...");
        //    await this.LoadPlayersAsync();
        //}

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true; 
            Debug.WriteLine("Loading players...");
            try
            {
                Players = await azureService.GetPlayers();
                Title = $"Players ({Players.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;

            }
        }

    }
}
