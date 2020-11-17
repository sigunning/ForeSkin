using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
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

        // player selected Command Interface called by control
        public ICommand SelectionCommand { private set; get; }
        public ICommand AddCommand { private set; get; }


        // works with the RefreshCommand and the IsRefreshing properties of the RefreshView.
        // Ensure that IsRefreshing is set to OneWay bind so it does not set the IsBusy flag.
        private Command refreshCommand;
        public Command RefreshCommand
        {
            get
            {
                return refreshCommand ?? (refreshCommand = new Command(async () => await ExecuteLoadItemsCommand(), () =>
                {
                    return !IsBusy;
                }));
            }
        }

        // selected item 
        private Player _selection;
        public Player Selection
        {
            get { return _selection; }
            set
            {
                _selection = value;
                OnPropertyChanged();
            }
        }

        // constructor
        public PlayerViewModel()
        {
            Title = "Players";
            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SelectionCommand = new Command<Player>(async (item) =>
            {
                if (item == null)
                {
                    return;
                }

                IsBusy = true;
                Debug.WriteLine(string.Concat("Player selected is: ", item.PlayerName));
                await Shell.Current.Navigation.PushAsync(new PlayerDetailPage(item));
                // un-select the list
                Selection = null;
                IsBusy = false;

            });

            AddCommand = new Command(async () =>
            {
                // create new course object with a guid id and pass to page
                Player player = new Player();
                player.PlayerId = Guid.NewGuid().ToString();
                IsBusy = true;
                Debug.WriteLine("Add player screen... ");
                await Shell.Current.Navigation.PushAsync(new PlayerDetailPage(player));
                IsBusy = false;

            });

            // subscribe to messaging so that updates in detail ViewModel can reflect back in the list
            SubscribeToMessageCenter();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        private void SubscribeToMessageCenter()
        {
            MessagingCenter.Subscribe<Player>(this, "AddNew",  (player) =>
            {
                Players.Add(player);
                Title = $"Players ({Players.Count})";
            });

            MessagingCenter.Subscribe<Player>(this, "Update", async (player) =>
            {
                // refresh list?
            });
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

        public async void OnDisplay()
        {
            base.OnAppearing();
            Debug.WriteLine("Loading players...");
            await ExecuteLoadItemsCommand();
        }

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
