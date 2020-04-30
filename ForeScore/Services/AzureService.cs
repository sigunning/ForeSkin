#define AUTH

using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;
using System.Diagnostics;
using ForeScore;
using ForeScore.Models;
using ForeScore.Helpers;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.Collections.ObjectModel;
//using Plugin.Connectivity;
using Xamarin.Essentials;


[assembly: Dependency(typeof(AzureService))]
namespace ForeScore
{
    public class AzureService
    {
        public MobileServiceClient client { get; set; } = null;

        // switch on authentication
        public static bool UseAuth { get; set; } = false;



        // use query ids in Pull
        Boolean _UseQueryId = false;
        string _queryId;

        // table vars
        IMobileServiceSyncTable<Player> tablePlayer;
        IMobileServiceSyncTable<Tournament> tableTournament;
        IMobileServiceSyncTable<Course> tableCourse;
        IMobileServiceSyncTable<Sledge> tableSledge;
        IMobileServiceSyncTable<Round> tableRound;
        IMobileServiceSyncTable<PlayerRound> tablePlayerRound;
        IMobileServiceSyncTable<CourseHoles> tableCourseHoles;
        IMobileServiceSyncTable<Scores> tableScores;

        #region Init

        public async Task Initialize()
        {
            if (client?.SyncContext?.IsInitialized ?? false)
                return;

            var appUrl = "http://pinscribeapp.azurewebsites.net/";


            //Create our client
            client = new MobileServiceClient(appUrl);


            //InitialzeDatabase for path
            var path = "forescore.db";
            path = Path.Combine(MobileServiceClient.DefaultDatabasePath, path);
              

            // delete store if setting
            if (Preferences.Get("ResetData", false) == true) 
            {
                // todo
                
                var fileHelper = DependencyService.Get<IFileHelper>();
                await fileHelper.DeleteLocalFileAsync(path);
                Preferences.Set("ResetData", false) ;
                
            }

            //setup our local sqlite store and intialize our table
            var store = new MobileServiceSQLiteStore(path);


            //Define tables - must be before sync context
            Debug.WriteLine("Defining local store tables... ");
            store.DefineTable<Player>();
            store.DefineTable<Tournament>();
            store.DefineTable<Course>();
            store.DefineTable<Round>();
            store.DefineTable<PlayerRound>();
            store.DefineTable<Sledge>();
            store.DefineTable<CourseHoles>();
            store.DefineTable<Scores>();


            //Initialize SyncContext
            Debug.WriteLine("Initializing mobile services SyncContext... ");
            await client.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // get mobile service sync (local) tables. All updates apply to local
            // tables, which are then pushed to server
            Debug.WriteLine("Setting Player table... ");
            tablePlayer = client.GetSyncTable<Player>();
            Debug.WriteLine("Setting Tournament table... ");
            tableTournament = client.GetSyncTable<Tournament>();
            Debug.WriteLine("Setting Course table... ");
            tableCourse = client.GetSyncTable<Course>();
            Debug.WriteLine("Setting CourseHoles table... ");
            tableCourseHoles = client.GetSyncTable<CourseHoles>();
            Debug.WriteLine("Setting Round table... ");
            tableRound = client.GetSyncTable<Round>();
            Debug.WriteLine("Setting PlayerRound table... ");
            tablePlayerRound = client.GetSyncTable<PlayerRound>();
            Debug.WriteLine("Setting Scores table... ");
            tableScores = client.GetSyncTable<Scores>();

            //Debug.WriteLine("Setting Sledge table... ");   
            //tableSledge = client.GetSyncTable<Sledge>();


            //purge data
            //await tableSledge.PurgeAsync();


            Debug.WriteLine("Finished Initialize");
        }

        #endregion Init

        /* NB. PullAsync (queryId, query)#
        Incremental Sync: the first parameter to the pull operation is a query name that is used only on the client. 
        If you use a non-null query name, the Azure Mobile SDK performs an incremental sync. 
        Each time a pull operation returns a set of results, the latest updatedAt timestamp from that 
        result set is stored in the SDK local system tables. Subsequent pull operations retrieve only 
        records after that timestamp.

        To use incremental sync, your server must return meaningful updatedAt values and must also support 
        sorting by this field. 
        However, since the SDK adds its own sort on the updatedAt field, you cannot use a pull query that 
        has its own orderBy clause.

        The query name can be any string you choose, but it must be unique for each logical query in your app. 
        Otherwise, different pull operations could overwrite the same incremental sync timestamp and your 
        queries can return incorrect results.
        */


        public async Task SyncAllData()
        {
            // Refresh all data. Push updates then pull all from server
            try
            {
                if (( Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true) )
                    return;

                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return; 

                // Push async operates on all tables, not just specific
                await Initialize();
                await client.SyncContext.PushAsync();
                _queryId =  null;
                await tablePlayer.PullAsync(_queryId, tablePlayer.CreateQuery());
                await tableTournament.PullAsync(_queryId, tableTournament.CreateQuery());
                await tableCourse.PullAsync(_queryId, tableCourse.CreateQuery());
                await tableRound.PullAsync(_queryId, tableRound.CreateQuery());
                await tableCourseHoles.PullAsync(_queryId, tableCourseHoles.CreateQuery());
                await tablePlayerRound.PullAsync(_queryId, tablePlayerRound.CreateQuery());
                await tableScores.PullAsync(_queryId, tableScores.CreateQuery());


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }



        #region Player

        public async Task<ObservableCollection<Player>> GetPlayers()
        {
            await Initialize();
            await SyncPlayers();
            return await tablePlayer
                .OrderBy(x => x.PlayerName)
                .ToCollectionAsync();
        }

        public async Task SyncPlayers()
        {
            try
            {

                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                // Push async operates on all tables, not just specific
                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allPlayers" : null);
                await tablePlayer.PullAsync(_queryId, tablePlayer.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task SavePlayerAsync(Player player)
        {
            await Initialize();

            try
            {

                if (player.Id == null)
                {
                    await tablePlayer.InsertAsync(player);
                }
                else
                {
                    await tablePlayer.UpdateAsync(player);
                }
                await SyncPlayers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save player: " + ex);
            }
        }


        #endregion Player


        #region Tournament
        

        public async Task SyncTournaments()
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allTournaments" : null);
                await tableTournament.PullAsync(_queryId, tableTournament.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task<ObservableCollection<Tournament>> GetTournaments()
        {
            await Initialize();
            await SyncTournaments();
            return await tableTournament
                .OrderBy(x => x.StartDate)
                .ToCollectionAsync();
            
        }

        public async Task SaveTournamentAsync(Tournament tournament)
        {
            await Initialize();

            try
            {

                if (tournament.Id == null)
                {
                    await tableTournament.InsertAsync(tournament);
                }
                else
                {
                    await tableTournament.UpdateAsync(tournament);
                }
                await SyncTournaments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save tournament: " + ex);
            }
        }

        #endregion Tournament


        #region Course

        public async Task SyncCourses()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allCourses" : null);
                await tableCourse.PullAsync(_queryId, tableCourse.CreateQuery());
                
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }

        }

        public async Task<ObservableCollection<Course>> GetCourses()
        {
            await Initialize();
            await SyncCourses();
            //IEnumerable<Course> items = await tableCourse.ToEnumerableAsync();
            return await tableCourse
                .OrderBy(x=> x.CourseName)
                .ToCollectionAsync();
        }
        public async Task<ObservableCollection<Course>> GetCourses(Boolean bLocalOnly)
        {
            if (!bLocalOnly)
            {
                await Initialize();
                await SyncCourses();
            }
            return await tableCourse
                .OrderBy(x => x.CourseName)
                .ToCollectionAsync();
        }


        public async Task SaveCourseAsync(Course course)
        {
            await Initialize();

            try
            {

                if (course.Id == null)
                {
                    await tableCourse.InsertAsync(course);
                }
                else
                {
                    await tableCourse.UpdateAsync(course);
                }
                await SyncCourses();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save course: " + ex);
            }
        }

        public async Task DeleteCourseAsync(Course course)
        {
            if (course.Id == null) return;

            await Initialize();

            try
            {

                await tableCourse.DeleteAsync(course);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete course: " + ex);
            }
        }

        #endregion Course


        #region Sledge

        public async Task SyncSledges()
        {
            try
            {
                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allSledges" : null);
                await tableSledge.PullAsync(_queryId, tableSledge.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task<List<Sledge>> GetSledges()
        {
            await Initialize();
            await SyncSledges();
            List<Sledge> lst = await tableSledge.ToListAsync();
            return await tableSledge.ToListAsync();

        }

        public async Task SaveSledgeAsync(Sledge sledge)
        {
            await Initialize();

            try
            {

                if (sledge.Id == null)
                {
                    await tableSledge.InsertAsync(sledge);
                }
                else
                {
                    await tableSledge.UpdateAsync(sledge);
                }
                await SyncSledges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save Sledge: " + ex);
            }
        }

        #endregion Sledge


        #region Round

        public async Task SyncRounds()
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allRounds" : null);
                await tableRound.PullAsync(_queryId, tableRound.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task<ObservableCollection<Round>> GetRounds()
        {
            await Initialize();
            await SyncRounds();
            return await tableRound
                .OrderBy(x =>x.RoundDate)
                .ToCollectionAsync();

        }

        public async Task<ObservableCollection<Round>> GetRounds(string course_id)
        {
            await Initialize();
            await SyncRounds();
            return await tableRound
                .Where(t => t.Course_id == course_id)
                .OrderBy(t => t.RoundDate)
                .ToCollectionAsync();

        }



        public async Task<ObservableCollection<Round>> GetTournamentRounds(string tournament_id)
        {
            await Initialize();
            await SyncRounds();
            return await tableRound
                .Where( t=> t.Tournament_id== tournament_id)
                .OrderBy(t => t.RoundDate)
                .ToCollectionAsync();

        }

        public async Task SaveRoundAsync(Round round)
        {
            await Initialize();

            try
            {

                if (round.Id == null)
                {
                    await tableRound.InsertAsync(round);
                }
                else
                {
                    await tableRound.UpdateAsync(round);
                }
                await SyncRounds();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save Round: " + ex);
            }
        }

        public async Task DeleteRoundAsync(Round round)
        {
            if (round.Id == null) return;

            await Initialize();

            try
            {

                await tableRound.DeleteAsync(round);
                await SyncRounds();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete Round: " + ex);
            }
        }

        #endregion Round


        #region PlayerRound

        public async Task SyncPlayerRounds()
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allPlayerRounds" : null);
                await tablePlayerRound.PullAsync(_queryId, tablePlayerRound.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }


        public async Task<ObservableCollection<PlayerRound>> GetPlayerRounds(string round_id)
        {
            await Initialize();
            await SyncPlayerRounds();
            return await tablePlayerRound
                .Where(t => t.Round_id == round_id)
                .ToCollectionAsync();

        }
       

        public async Task SavePlayerRoundAsync(PlayerRound playerRound)
        {
            await Initialize();

            try
            {

                if (playerRound.Id == null)
                {
                    await tablePlayerRound.InsertAsync(playerRound);
                }
                else
                {
                    await tablePlayerRound.UpdateAsync(playerRound);
                }
                await SyncPlayerRounds();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save PlayerRound: " + ex);
            }
        }

        public async Task SaveAllPlayerRoundAsync(ObservableCollection<PlayerRound> allPlayerRounds)
        {
            await Initialize();

            try
            {
                foreach (PlayerRound playerRound in allPlayerRounds)
                {
                    if (playerRound.Id == null)
                    {
                        await tablePlayerRound.InsertAsync(playerRound);
                    }
                    else
                    {
                        await tablePlayerRound.UpdateAsync(playerRound);
                    }
                }
            
                await SyncPlayerRounds();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save PlayerRounds: " + ex);
            }
        }

        public async Task DeletePlayerRoundAsync(PlayerRound playerRound)
        {
            if (playerRound.Id == null) return;

            await Initialize();

            try
            {

                await tablePlayerRound.DeleteAsync(playerRound);
                await SyncRounds();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete PlayerRound: " + ex);
            }
        }


        #endregion PlayerRound


        #region CourseHoles

        public async Task SyncCourseHoles()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allCourseHoles" : null);
                await tableCourseHoles.PullAsync(_queryId, tableCourseHoles.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing allCourseHoles sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }

        }

        public async Task<ObservableCollection<CourseHoles>> GetCourseHoles()
        {
            await Initialize();
            await SyncCourseHoles();
            return await tableCourseHoles
                .OrderBy(x => x.HoleNumber)
                .ToCollectionAsync();

        }

        public async Task<ObservableCollection<CourseHoles>> GetCourseCourseHoles(string course_id)
        {
            await Initialize();
            await SyncCourseHoles();
            return await tableCourseHoles
                .Where(t => t.Course_id == course_id)
                .OrderBy(x => x.HoleNumber)
                .ToCollectionAsync();

        }

        public async Task SaveCourseCourseHolesAsync(ObservableCollection<CourseHoles> courseCourseHoles)
        {
            await Initialize();
            await SyncCourseHoles();
            try
            {

                foreach (var ch in courseCourseHoles)
                {
                    await SaveCourseHolesAsync(ch);
                }
                await SyncCourseHoles();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save CourseHoles: " + ex);
            }
        }

        public async Task SaveCourseHolesAsync(CourseHoles courseHoles)
        {
            //await Initialize();

            try
            {

                if (courseHoles.Id == null)
                {
                    await tableCourseHoles.InsertAsync(courseHoles);
                }
                else
                {
                    await tableCourseHoles.UpdateAsync(courseHoles);
                }
                //await SyncCourseHoles();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save CourseHoles: " + ex);
            }
        }

        #endregion CourseHoles


        #region Scores

        public async Task SyncScores(string playerRound_id)
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "playerRoundScores" : null);
                await tableScores.PullAsync(_queryId, tableScores.Where(p => p.PlayerRound_id == playerRound_id) );
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task SyncScores(List<string> lstPlayerRound_id)
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "listPlayerRoundScores" : null);
                await tableScores.PullAsync(_queryId, tableScores.Where(t => lstPlayerRound_id.Contains(t.PlayerRound_id)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }


        public async Task SyncScores()
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allScores" : null);
                await tableScores.PullAsync(_queryId, tableScores.CreateQuery() );
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task<List<Scores>> GetScores(string playerRound_id)
        {
            await Initialize();
            await SyncScores(playerRound_id);
            return await tableScores
                .Where(t => t.PlayerRound_id == playerRound_id)
                .OrderBy(o=>o.Hole)
                .ToListAsync();

        }

        public async Task<List<Scores>> GetHoleScores(List<String> lstPlayerRound_id, int holeNumber)
        {
            // local only
            //await Initialize();
            //await SyncScores(playerRound_id);
            return await tableScores
                .Where(t => t.Hole== holeNumber && lstPlayerRound_id.Contains(t.PlayerRound_id))
                .ToListAsync();

        }

        public async Task<List<Scores>> GetHoleScores(List<String> lstPlayerRound_id, int holeNumber, bool holeToDate=false)
        {
            // local only
            //await Initialize();
            //await SyncScores(playerRound_id);
            if (holeToDate)
            {
                return await tableScores
                    .Where(t => t.Hole <= holeNumber && lstPlayerRound_id.Contains(t.PlayerRound_id))
                    .ToListAsync();
            }
            else
            {
                return await tableScores
                    .Where(t => t.Hole == holeNumber && lstPlayerRound_id.Contains(t.PlayerRound_id))
                    .ToListAsync();
            }
        }


        public async Task<List<Scores>> GetScores()
        {
            await Initialize();
            await SyncScores();
            return await tableScores
                .ToListAsync();
        }


        public async Task CreateScores(string playerRound_id)
        {
            await Initialize();
            await SyncScores(playerRound_id);

            for (int i = 1; i <= 18; i++)
            {
                Scores score = new Scores() { Hole = i, PlayerRound_id=playerRound_id };
                await SaveScore(score);
            }


        }

        public async Task SaveScores(List<Scores> lstScores)
        {
            // save list of scores
            List<string> lstPlayerRound_id = new List<string>();
            foreach (Scores score in lstScores )
            {
                lstPlayerRound_id.Add(score.PlayerRound_id);
                await SaveScore(score);
            }

            // sync these players only
            await SyncScores(lstPlayerRound_id);
        }

        public async Task SaveScore(Scores scores)
        {
            try
            {

                if (scores.Id == null)
                {
                    await tableScores.InsertAsync(scores);
                }
                else
                {
                    await tableScores.UpdateAsync(scores);
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save Score: " + ex);
            }
        }

        public async void GetRoundScores(List<PlayerRound> playerRounds)
        {

            // ######### not in use #######
            await Initialize();
           // await SyncScores();

            // list to hold all players scores
            List<PlayerRoundScore> playerRoundScore = new List<PlayerRoundScore>();

            // ensure we have score records for each player round
            foreach (PlayerRound pr in playerRounds)
            {
                string playerRound_id = pr.Id;

                List<Scores> scores = await GetScores(playerRound_id);
                if (scores.Count<18)
                {
                    // delete any

                    // create score set
                    CreateScores(playerRound_id);
                    scores = await GetScores(playerRound_id);
                }

                // now add score to playerRoundScore list

                //playerRoundScore = (from c1 in scores
                //                join c2 in lstCourses
                //                on c1.Course_id equals c2.Id
                //                select new LookupRound { Id = c1.Id, Name = c2.CourseName }).ToList();




            }
        }

        #endregion Scores

    }
}