using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.Models;
using ForeScore.Views;
using ForeScore.ViewModels;
using ForeScore;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using ForeScore.Common;
using ForeScore.Helpers;
using System.Collections.ObjectModel;
using ForeScore.Controls;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    // inherit from PlayerRoundViewModel

    class ReportsViewModel : PlayerRoundViewModel
    {

        // service and property vars
        //private AzureService azureService;
        private INavigation _navigation;
        private List<Player> _RoundPlayers;
        private List<PlayerRoundScore> _playerRoundScores;
        private ObservableCollection<CourseHoles> _courseHoles;



        public ReportsViewModel(INavigation navigation) : base(navigation)
        {
            _navigation = navigation;
            Title = "Reports";
            ThisPlayer = new Player();
        }

        // for new player picker list
        public List<Player> RoundPlayers
        {
            get { return _RoundPlayers; }
            set
            {
                _RoundPlayers = value;
                OnPropertyChanged();
            }
        }

        // Property for  player picker list
        private Player _thisPlayer;
        public Player ThisPlayer
        {
            get { return _thisPlayer; }
            set
            {
                _thisPlayer = value;
                OnPropertyChanged();
            }
        }



        public async void RoundSelected(object sender, SelectedItemChangedEventArgs e)
        {

            if (e == null) return; // has been set to null, do not 'process' tapped event

            if (IsBusy) return;
            IsBusy = true;

            ThisRound.Id = ((LookupRound)e.SelectedItem).Id;
            Debug.WriteLine(string.Concat("Round selected: ", ((LookupRound)e.SelectedItem).Name));

            // store to settings

            Preferences.Set("Round_id", ThisRound.Id);
            Preferences.Set("CourseName", ((LookupRound)e.SelectedItem).Name);
            Preferences.Set("Course_id", ((LookupRound)e.SelectedItem).Course_id);

            // load players with PlayerRound_id as key
            PlayerRounds = await azureService.GetPlayerRounds(ThisRound.Id);
            Debug.WriteLine(string.Concat("Player count: ", PlayerRounds.Count.ToString()));

            if (PlayerRounds != null)
            {
                RoundPlayers = (from P in base.Players
                                join PR in base.PlayerRounds
                                on P.Id equals PR.Player_id
                                select new Player { Id = PR.Id, PlayerName = P.PlayerName }).ToList();
            }

            // get course holes
            _courseHoles = await azureService.GetCourseCourseHoles(Preferences.Get("Course_id", null));


            IsBusy = false;


        }

        public async void PlayerSelected(object sender, SelectedItemChangedEventArgs e)
        {

            if (e == null) return; // has been set to null, do not 'process' tapped event

            // store PlayerRound_id (not Player ID!)
            ThisPlayer.Id = ((Player)e.SelectedItem).Id;
            ThisPlayer.PlayerName = ((Player)e.SelectedItem).PlayerName;

        }




        public Command CardCommand
        {
            get
            {

                return new Command(async (e) =>
                {
                    if ((ThisPlayer.Id == null) || (ThisRound.Id == null) || (ThisRound.Tournament_id == null)) return;

                    if (IsBusy) return;
                    IsBusy = true;

                    // delete logic on item
                    Debug.WriteLine(string.Concat("Report for player: ", ThisPlayer.PlayerName));

                    // get scores for this player
                    List<Scores> scores = await azureService.GetScores(ThisPlayer.Id);

                    // build score object sfor display and updating
                    // ## TODO: Pass HcapPct to GetShots()
                    List<ScoreCardHole> lstScoreCardHoles = new List<ScoreCardHole>();
                    lstScoreCardHoles = (from s1 in scores
                                            join h1 in _courseHoles on s1.Hole equals h1.HoleNumber
                                            join p1 in PlayerRounds on s1.PlayerRound_id equals p1.Id
                                            select new ScoreCardHole
                                            {
                                                
                                                PlayerRound_id = s1.PlayerRound_id,
                                                CourseName = Preferences.Get("CourseName", null),
                                                Hole = s1.Hole,
                                                HoleName = s1.Hole.ToString(),
                                                Gross = s1.Gross,
                                                HolePar = h1.HolePar,
                                                HoleSI = h1.HoleSI,
                                                HCAP = p1.HCAP,
                                                PlayerName = Lookups._dictPlayers[p1.Player_id],
                                                Strokes = Utils.GetShots(p1.HCAP, h1.HoleSI),
                                                Net = s1.Gross==0 ? 0 : s1.Gross - Utils.GetShots(p1.HCAP, h1.HoleSI),
                                                Points = s1.Gross == 0 ? 0 :  Utils.GetPoints(s1.Gross, h1.HolePar, p1.HCAP, h1.HoleSI),
                                                StrokeStars = new String('*', Utils.GetShots(p1.HCAP, h1.HoleSI))
                                            }).OrderBy(o => o.Hole)
                                            .ToList();

                    // calc cumulative points for each player
                    foreach (ScoreCardHole prc in lstScoreCardHoles)
                    {
                        prc.CumPoints = lstScoreCardHoles.Where(o => o.Hole <= prc.Hole).Sum(o => o.Points);
                        
                    }

                    // front nine totals
                    ScoreCardHole prcOut = new ScoreCardHole()
                    {
                        PlayerRound_id = ThisPlayer.Id,
                        HoleName = "OUT",
                        Hole=0
                    };
                    prcOut.HolePar = lstScoreCardHoles.Where(o => o.Hole <= 9).Sum(o => o.HolePar);
                    prcOut.Gross = lstScoreCardHoles.Where(o => o.Hole <= 9).Sum(o => o.Gross);
                    prcOut.Net = lstScoreCardHoles.Where(o => o.Hole <= 9).Sum(o => o.Net);
                    prcOut.Points = lstScoreCardHoles.Where(o => o.Hole <= 9).Sum(o => o.Points);
                    prcOut.CumPoints= prcOut.Points;
                    lstScoreCardHoles.Insert(9, prcOut);

                    // back nine totals
                    ScoreCardHole prcIn= new ScoreCardHole()
                    {
                        PlayerRound_id = ThisPlayer.Id,
                        HoleName = "IN",
                        Hole = 0
                    };
                    prcIn.HolePar = lstScoreCardHoles.Where(o => o.Hole > 9).Sum(o => o.HolePar);
                    prcIn.Gross = lstScoreCardHoles.Where(o => o.Hole > 9).Sum(o => o.Gross);
                    prcIn.Net = lstScoreCardHoles.Where(o => o.Hole > 9).Sum(o => o.Net);
                    prcIn.Points = lstScoreCardHoles.Where(o => o.Hole > 9).Sum(o => o.Points);
                    prcIn.CumPoints = prcIn.Points;
                    lstScoreCardHoles.Add(prcIn);
                    

                    // 18 totals
                    ScoreCardHole prcTotal = new ScoreCardHole()
                    {
                        PlayerRound_id = ThisPlayer.Id,
                        HoleName = "TOT"
                    };
                    prcTotal.HolePar = lstScoreCardHoles.Where(o => o.Hole == 0).Sum(o => o.HolePar);
                    prcTotal.Gross = lstScoreCardHoles.Where(o => o.Hole == 0).Sum(o => o.Gross);
                    prcTotal.Net = lstScoreCardHoles.Where(o => o.Hole == 0).Sum(o => o.Net);
                    prcTotal.Points = lstScoreCardHoles.Where(o => o.Hole == 0).Sum(o => o.Points);
                    prcTotal.CumPoints = prcTotal.Points;
                    lstScoreCardHoles.Add(prcOut);
                    lstScoreCardHoles.Add(prcTotal);

            //        await _navigation.PushAsync(new ScorecardPage(lstScoreCardHoles));


                    IsBusy = false;

                });
            }
        }

        public Command RoundCommand
        {
            get
            {

                return new Command(async (e) =>
                {
                    if ((ThisRound.Id == null) || (ThisRound.Tournament_id == null)) return;

                    if (IsBusy) return;
                    IsBusy = true;

                    // delete logic on item
                    Debug.WriteLine(string.Concat("Report for round: ", Preferences.Get("CourseName",null) ));

                    // get scores for current hole for all players in group
                    List<String> lstPlayerRound_id = PlayerRounds.Select(o => o.Id).ToList<string>();
                    List<Scores> scores = await azureService.GetHoleScores(lstPlayerRound_id, 18, true);


                    // build score object sfor display and updating
                    // ## TODO: Pass HcapPct to GetShots()
                    List<PlayerRoundScore> lstPlayerRoundScores = new List<PlayerRoundScore>();
                    lstPlayerRoundScores = (from s1 in scores
                                            join h1 in _courseHoles on s1.Hole equals h1.HoleNumber
                                            join p1 in PlayerRounds on s1.PlayerRound_id equals p1.Id
                                            select new PlayerRoundScore
                                            {
                                                Id = s1.Id,
                                                PlayerRound_id = s1.PlayerRound_id,
                                                CourseName = Preferences.Get("CourseName", null),
                                                Hole = s1.Hole,
                                                Gross = s1.Gross,
                                                HolePar = h1.HolePar,
                                                HoleSI = h1.HoleSI,
                                                HCAP = p1.HCAP,
                                                PlayerName = Lookups._dictPlayers[p1.Player_id],
                                                Strokes = Utils.GetShots(p1.HCAP, h1.HoleSI)
                                                //Points = Utils.GetPoints(s1.Gross, h1.HolePar, p1.HCAP, h1.HoleSI),

                                            }).OrderBy(o => o.Hole)
                                            .ToList();

                    // calc cumulative points for each player. Just update the final hole
                    foreach (PlayerRoundScore prc in lstPlayerRoundScores.Where(o => o.Hole == 18))
                    {
                        prc.CumGross = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Gross);
                        prc.CumNet = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Net);
                        prc.CumPoints = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Points);


                    }
                    // just last hole
                    lstPlayerRoundScores = lstPlayerRoundScores.Where(o => o.Hole == 18).OrderBy(o => o.CumPoints).ToList<PlayerRoundScore>();
                    await _navigation.PushAsync(new ResultsPage(lstPlayerRoundScores));


                    IsBusy = false;

                });
            }
        }

        public Command TournamentCommand
        {
            get
            {

                return new Command(async (e) =>
                {
                    if ((ThisRound.Tournament_id == null)) return;

                    if (IsBusy) return;
                    IsBusy = true;

                    // delete logic on item
                    Debug.WriteLine(string.Concat("Report for tournament: ", Lookups._dictTournaments[Preferences.Get("Tournament_id", null)]));

                    // lstRounds holds all rounds for tournie, so get all playerrounds
                    string lastRoundId= ThisRound.Id;
                    List<String> lstPlayerRound_id = new List<string>();
                    // create course holes
                    List<CourseHoles> _tournamentCourseHoles = new List<CourseHoles>();
                    List<PlayerRound> _tournamentPlayerRounds = new List<PlayerRound>();
                    foreach (Round round in lstRounds.OrderBy(o => o.RoundDate) )
                    {
                        PlayerRounds = await azureService.GetPlayerRounds(round.Id);
                        lstPlayerRound_id.AddRange(PlayerRounds.Select(o => o.Id).ToList<string>());
                        _tournamentPlayerRounds.AddRange(PlayerRounds);
                        lastRoundId = round.Id;

                        // collate course holes
                        _tournamentCourseHoles.AddRange ( await azureService.GetCourseCourseHoles(round.Course_id) );
                    }


                    // get scores for all players
                    List<Scores> scores = await azureService.GetHoleScores(lstPlayerRound_id, 18, true);

                

                    // build score object sfor display and updating
                    // ## TODO: Pass HcapPct to GetShots()
                    List<PlayerRoundScore> lstPlayerRoundScores = new List<PlayerRoundScore>();
                    lstPlayerRoundScores = (from s1 in scores
                                            join p1 in _tournamentPlayerRounds on s1.PlayerRound_id equals p1.Id
                                            join r1 in lstRounds on  p1.Round_id equals r1.Id
                                            join h1 in _tournamentCourseHoles on new { HoleNumber=s1.Hole, r1.Course_id  } equals new { h1.HoleNumber, h1.Course_id }
                                            select new PlayerRoundScore
                                            {
                                                Id = s1.Id,
                                                PlayerRound_id = s1.PlayerRound_id,
                                                Round_id = p1.Round_id,
                                                Player_id= p1.Player_id,
                                                CourseName = "Overall",
                                                Hole = s1.Hole,
                                                Gross = s1.Gross,
                                                HolePar = h1.HolePar,
                                                HoleSI = h1.HoleSI,
                                                HCAP = p1.HCAP,
                                                PlayerName = Lookups._dictPlayers[p1.Player_id],
                                                Strokes = Utils.GetShots(p1.HCAP, h1.HoleSI)
                                                //Points = Utils.GetPoints(s1.Gross, h1.HolePar, p1.HCAP, h1.HoleSI),

                                            }).OrderBy(o => o.PlayerRound_id).OrderBy(o => o.Hole)
                                            .ToList();

                    // calc cumulative points for each player. Just update the final hole
                    foreach (PlayerRoundScore prc in lstPlayerRoundScores.Where(o => o.Hole == 18))
                    {
                        prc.CumGross = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Gross);
                        prc.CumNet = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Net);
                        prc.CumPoints = lstPlayerRoundScores.Where(o => o.Hole <= prc.Hole && o.PlayerRound_id == prc.PlayerRound_id).Sum(o => o.Points);


                    }
                    // just last hole for each round
                    lstPlayerRoundScores = lstPlayerRoundScores.Where(o => o.Hole == 18).ToList<PlayerRoundScore>();

                    // now sum by player and store against last round lastRoundId
                    foreach (PlayerRoundScore prc in lstPlayerRoundScores.Where(o => o.Round_id== lastRoundId))
                    {
                        prc.Gross = lstPlayerRoundScores.Where(o=> o.Player_id == prc.Player_id).Sum(o => o.CumGross);
                        prc.GrossOriginal = lstPlayerRoundScores.Where(o => o.Player_id == prc.Player_id).Sum(o => o.CumNet);
                        prc.Strokes = lstPlayerRoundScores.Where(o => o.Player_id == prc.Player_id).Sum(o => o.CumPoints);
                    }
                    foreach (PlayerRoundScore prc in lstPlayerRoundScores.Where(o => o.Round_id == lastRoundId))
                    {
                        prc.CumGross = prc.Gross;
                        prc.CumNet = prc.GrossOriginal;
                        prc.CumPoints = prc.Strokes;
                    }

                    // just last round
                    lstPlayerRoundScores = lstPlayerRoundScores.Where(o => o.Round_id == lastRoundId).OrderBy(o => o.CumPoints).ToList<PlayerRoundScore>();
                    await _navigation.PushAsync(new ResultsPage(lstPlayerRoundScores));


                    IsBusy = false;

                });
            }
        }

    }


}
