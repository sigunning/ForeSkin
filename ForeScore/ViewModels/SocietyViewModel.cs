using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using ForeScore.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

namespace ForeScore.ViewModels
{
    class SocietyViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        public ICommand AddSocietiesCommand { private set; get; }

        public ICommand SelectionCommand { private set; get; }
        public ICommand LoadItemsCommand { get; }

        public SocietyViewModel()
        {
            azureService = DependencyService.Get<AzureService>();

            Title = "Societies";

            // Command implementation
            // implement the ICommands
            SelectionCommand = new Command<Society>(async (item) =>
            {
                if (item == null)
                {
                    return;
                }

                IsBusy = true;
                Debug.WriteLine(string.Concat("Society selected is: ", item.SocietyName));
                await Shell.Current.Navigation.PushAsync(new SocietyDetailPage(item));
                // un-select the list
                SelectedSociety = null;
                IsBusy = false;

            });

            AddSocietiesCommand = new Command(async () =>
            {
                IsBusy = true;
                // create new society object
                Society item = new Society();
                item.SocietyId = Guid.NewGuid().ToString();
                item.CreatedByPlayerId = Preferences.Get("PlayerId", null);
                item.CreatedDate = DateTime.Now;
                await Shell.Current.Navigation.PushAsync(new SocietyDetailPage(item));
                IsBusy = false;

            });

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            // subscribe to messaging so that updates in detail ViewModel can reflect back in the list
            SubscribeToMessageCenter();

        }

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


        private void SubscribeToMessageCenter()
        {
            MessagingCenter.Subscribe<Society>(this, "AddNew", (society) =>
            {
                Societies.Add(society);
                Title = $"Societies ({Societies.Count})";
            });

            MessagingCenter.Subscribe<Society>(this, "Update", async (society) =>
            {
                // refresh list?
            });
        }

        // Selected Society
        private Society _selectedSociety;
        public Society SelectedSociety
        {
            get { return _selectedSociety; }
            set
            {
                _selectedSociety = value;
                OnPropertyChanged();
                
            }
        }

        // List of societies
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


        // load societies for current player
        public async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //DoStuff
            Societies = await azureService.GetSocieties(Preferences.Get("PlayerId",null) );

            Title = $"Societies ({Societies.Count})";

            IsBusy = false;

           

        }

        public void LoadData()
        {
            ExecuteLoadItemsCommand();
        }
    }
}
