using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForeScore.ViewModels;
using ForeScore.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;


namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScoresPage : ContentPage
    {
        public ScoresPage(ObservableCollection<PlayerRound> playerRounds)
        {
            InitializeComponent();
            // get viewmodel and set selected Player 
            ScoresViewModel vm = new ScoresViewModel(this.Navigation);
            // pass playerRounds to vm
            vm.playerRounds = playerRounds;

            BindingContext = vm;
        }

        private ScoresViewModel viewModel
        {
            get { return BindingContext as ScoresViewModel; }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnAppearing();

        }

        void OnHoleSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnHoleSelected(sender, e);
        }


    }
}
