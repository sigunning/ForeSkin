using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class PlayerScoreViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

       

        public PlayerScoreViewModel()
        {
            Title = "Player Setup";

            azureService = DependencyService.Get<AzureService>();


        }

        private Round _round;
        public Round Round 
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
            }
        }


        public async Task LoadData()
        {
            await azureService.LoadCourseLookup();
           

        }

    }
}
