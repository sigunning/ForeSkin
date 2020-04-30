using ForeScore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ForeScore.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlayerPage : ContentPage
	{
		public PlayerPage ()
		{
			InitializeComponent ();
			BindingContext = new PlayerViewModel();
		}
		private PlayerViewModel viewModel
		{
			get { return BindingContext as PlayerViewModel; }
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (viewModel == null)
				return;
			if (viewModel.Players == null)
				viewModel.LoadItemsCommand.Execute(null);

		}

		private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			// filter using linq
		}

		async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			// 
			lstPlayers.SelectedItem = null;
		}
	}
}