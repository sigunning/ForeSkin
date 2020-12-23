using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;
using System.Diagnostics;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScoreEntryPage : ContentPage
    {
        private Round round;
        public ScoreEntryPage(Round round)
        {
            InitializeComponent();

            BindingContext = new ScoreEntryViewModel();
            Title = round.CourseName;
            // assign Round property in viewmodel
            viewModel.Round = round;
        }

        private ScoreEntryViewModel viewModel
        {
            get { return BindingContext as ScoreEntryViewModel; }
        }

        protected override async void OnAppearing()
        {
            Debug.WriteLine("ScoreEntryPage OnAppearing");
            base.OnAppearing();
            if (viewModel == null)
                return;

            await viewModel.LoadData();

        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // change binding context of score entry to selected hole score
            //lstPlayers.ScoreEntry.SetBinding(Label.TextProperty, "S2");
            Debug.WriteLine("Hole changed");
            
        }
    }
}