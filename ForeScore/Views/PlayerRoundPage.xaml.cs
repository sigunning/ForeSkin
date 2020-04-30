using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.ViewModels;


namespace ForeScore.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlayerRoundPage : ContentPage
	{
		public PlayerRoundPage ()
		{
			InitializeComponent ();
            // get viewmodel and set selected Player 
            PlayerRoundViewModel vm = new PlayerRoundViewModel(this.Navigation);
            BindingContext = vm;
        }

        private PlayerRoundViewModel viewModel
        {
            get { return BindingContext as PlayerRoundViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnAppearing();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            viewModel.OnDisappearing();

        }


        void OnTournamentSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnTournamentSelected(sender, e);
        }

        void OnRoundSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnRoundSelected(sender, e);
        }
        void OnPlayerSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnPlayerSelected(sender, e);

        }
    }
}