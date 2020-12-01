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
    public partial class SyncPage : ContentPage
    {
        public SyncPage()
        {
            InitializeComponent();
            BindingContext = new SyncViewModel();
        }

        private SyncViewModel viewModel
        {
            get { return BindingContext as SyncViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null)
                return;

            // show latest connected mode
            viewModel.SetMode();
     

       

        }

        private void SwitchCell_OnChanged(object sender, ToggledEventArgs e)
        {
            // if reset switched on, set all others
            var sw = (SwitchCell)sender;
            if (sw.Text.Contains("Clear"))
            {
                if (e.Value == true)
                {
                    // set others on 
                    scScores.On = true;
                    scCompetitions.On = true;
                    scPlayers.On = true;
                    scSocieties.On = true;
                    scCourses.On = true;
                }
            }
            else
            {
                if (e.Value == false)
                {
                    scReset.On = false;
                }
                
            }
        }
    }
}