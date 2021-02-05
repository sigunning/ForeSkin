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
    public partial class SocietyPage : ContentPage
    {
        public SocietyPage()
        {
            InitializeComponent();
            BindingContext = new SocietyViewModel();
        }

        private SocietyViewModel viewModel
        {
            get { return BindingContext as SocietyViewModel; }
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (viewModel == null)
				return;

			// load list
			// if (viewModel.Societies == null)
			viewModel.LoadData();
			
		}

		private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			// filter using linq
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
				lstSocieties.ItemsSource = viewModel.Societies;
			else
				lstSocieties.ItemsSource = viewModel.Societies.Where(i => i.SocietyName.ToLower().Contains(e.NewTextValue.ToLower()));
		}
	}
}