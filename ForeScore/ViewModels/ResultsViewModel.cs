using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using ForeScore.Models;
using ForeScore.Helpers;
using ForeScore.Common;
using ForeScore;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class ResultsViewModel : BaseViewModel
    {

        private INavigation _navigation;
        private List<PlayerRoundScore> _playerRoundScores;

        public ResultsViewModel(INavigation navigation)
        {
            Title = "Results";
            _navigation = navigation;


        }

        public List<PlayerRoundScore> PlayerRoundScores
        {
            get { return _playerRoundScores; }
            set
            {
                _playerRoundScores = value;
                OnPropertyChanged();
            }
        }

        private string _courseName;
        public string CourseName
        {
            get { return _courseName; }
            set
            {
                _courseName = value;
                OnPropertyChanged();
            }
        }

        private string _tournamentName;
        public string TournamentName
        {
            get { return _tournamentName; }
            set
            {
                _tournamentName = value;
                OnPropertyChanged();
            }
        }




        public async void OnAppearing()
        {
            base.OnAppearing();
            CourseName = PlayerRoundScores.ElementAt(0).CourseName;
            TournamentName = Lookups._dictTournaments[Preferences.Get("Tournament_id", null)];
        }



    }
}
