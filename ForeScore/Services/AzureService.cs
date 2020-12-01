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
using ForeScore.Common;
//using Plugin.Connectivity;
using Xamarin.Essentials;
using System.Linq;

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
        IMobileServiceSyncTable<Society> tableSociety;
        IMobileServiceSyncTable<SocietyPlayer> tableSocietyPlayer;
        IMobileServiceSyncTable<Competition> tableCompetition;
        IMobileServiceSyncTable<Course> tableCourse;
        IMobileServiceSyncTable<Sledge> tableSledge;
        IMobileServiceSyncTable<Round> tableRound;
        IMobileServiceSyncTable<PlayerScore> tablePlayerScore;
        IMobileServiceSyncTable<PlayerRound> tablePlayerRound;
        
        IMobileServiceSyncTable<Scores> tableScores;

        IMobileServiceSyncTable<CourseHoles> tableCourseHoles;

        #region Init

        public async Task Initialize()
        {
            if (  (client?.SyncContext?.IsInitialized ?? false) && ((Preferences.Get("ResetData", false) == false))  )
                return;

            //var appUrl = "http://pinscribeapp.azurewebsites.net/";
            var appUrl = "http://forescore-rtg.azurewebsites.net/";


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
               
                
            }
            Preferences.Set("ResetData", false);

            //setup our local sqlite store and intialize our table
            var store = new MobileServiceSQLiteStore(path);


            //Define tables - must be before sync context
            Debug.WriteLine("Defining local store tables... ");
            store.DefineTable<Player>( );
            store.DefineTable<Competition>();
            store.DefineTable<Course>();
            store.DefineTable<Round>();
            store.DefineTable<PlayerScore>();
            store.DefineTable<PlayerRound>();
            store.DefineTable<Sledge>();
            store.DefineTable<Society>();
            store.DefineTable<SocietyPlayer>();
            store.DefineTable<Scores>();


            //Initialize SyncContext
            Debug.WriteLine("Initializing mobile services SyncContext... ");
            await client.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // get mobile service sync (local) tables. All updates apply to local
            // tables, which are then pushed to server
            Debug.WriteLine("Setting Player table... ");
            tablePlayer = client.GetSyncTable<Player>();
            Debug.WriteLine("Setting Course table... ");
            tableCourse = client.GetSyncTable<Course>();
            Debug.WriteLine("Setting Competition table... ");
            tableCompetition = client.GetSyncTable<Competition>();
            Debug.WriteLine("Setting Society table... ");
            tableSociety = client.GetSyncTable<Society>();
            Debug.WriteLine("Setting SocietyPlayer table... ");
            tableSocietyPlayer = client.GetSyncTable<SocietyPlayer>();
            Debug.WriteLine("Setting Round table... ");
            tableRound = client.GetSyncTable<Round>();
            Debug.WriteLine("Setting PlayerScore table... ");
            tablePlayerScore = client.GetSyncTable<PlayerScore>();
            /*
            
           
            
            Debug.WriteLine("Setting PlayerRound table... ");
            tablePlayerRound = client.GetSyncTable<PlayerRound>();
            Debug.WriteLine("Setting Scores table... ");
            tableScores = client.GetSyncTable<Scores>();
            */

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


        public async Task SyncAllData(SyncOptions SyncOptionsObj )

        {
            // Refresh all data. Push updates then pull all from server
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;
            try
            {
                if (( Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true) )
                    return;

                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return; 

                Debug.WriteLine("Starting SyncAllData..." );

                // set reset flag for Init ?
                if (SyncOptionsObj.Reset)
                    Preferences.Set("ResetData", true);

                // Initalize client context and local tables
                await Initialize();

                // everything gets pushed
                Debug.WriteLine("Pushing updates to remote server...");
                await client.SyncContext.PushAsync();

                Debug.WriteLine("Pulling updates from remote server...");
                _queryId =  null;
                if (SyncOptionsObj.Courses)
                    { await tableCourse.PullAsync(_queryId, tableCourse.CreateQuery());  }
                if (SyncOptionsObj.Players)
                    {   await tablePlayer.PullAsync(_queryId, tablePlayer.CreateQuery());  }
                if (SyncOptionsObj.Societies)
                    { await tableSociety.PullAsync(_queryId, tableSociety.CreateQuery());
                    await tableSocietyPlayer.PullAsync(_queryId, tableSocietyPlayer.CreateQuery());
                    }
                if (SyncOptionsObj.Competitions)
                {
                    await tableCompetition.PullAsync(_queryId, tableCompetition.CreateQuery());
                    await tableRound.PullAsync(_queryId, tableRound.CreateQuery());
                }
                if (SyncOptionsObj.Scores)
                { 
                    await tablePlayerScore.PullAsync(_queryId, tablePlayerScore.CreateQuery());
                }

                
                /*
              
             

              await tableTournament.PullAsync(_queryId, tableTournament.CreateQuery());
             
              await tableCourseHoles.PullAsync(_queryId, tableCourseHoles.CreateQuery());
              await tablePlayerRound.PullAsync(_queryId, tablePlayerRound.CreateQuery());
              await tableScores.PullAsync(_queryId, tableScores.CreateQuery());
              */

                Debug.WriteLine("Completed SyncAllData");
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
                        Debug.WriteLine(error.Result);
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        Debug.WriteLine(error.Result);
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
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

        

        public async Task<Player> GetPlayer(string playerId)
        {
            await Initialize();
            await SyncPlayers();
            ObservableCollection<Player> results = await tablePlayer
                .Where(x => x.PlayerId == playerId)
                .ToCollectionAsync();
            return results.SingleOrDefault();
            
               
        }

        public async Task LoadPlayerLookup(bool reload=false)
        {
            // if we have already loaded courses to picker lists, just return new collection
            if ((Lookups._dictPlayers.Count == 0) || (reload=true))
                {
                ObservableCollection<Player> lstPlayers;
                Debug.WriteLine("Loading player lookup list...");
                lstPlayers = await GetPlayers();
                Lookups._dictPlayers = lstPlayers.ToDictionary(x => x.PlayerId, x => x.PlayerName);
                Pickers.PickerPlayer = lstPlayers.ToList();
            }
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

        #region Society

        public async Task<ObservableCollection<Society>> GetSocieties()
        {
            await Initialize();
            return await tableSociety
                .OrderBy(x => x.SocietyName)
                .ToCollectionAsync();
        }
        public async Task<ObservableCollection<Society>> GetSocieties(string playerId)
        {
            // need to get societies where player is a member - join to SocietyPlayer
            await Initialize();
            ObservableCollection<Society> societies = await GetSocieties();
            ObservableCollection<SocietyPlayer> societyPlayers = await GetSocietyPlayers();
            List<Society> lstPlayerSocieties = new List<Society>();
            lstPlayerSocieties = (from s1 in societies 
                     join sp1 in societyPlayers.Where(o => o.PlayerId == playerId) on s1.SocietyId equals sp1.SocietyId
                     select new Society
                     {
                         Id = s1.Id,
                         SocietyId = s1.SocietyId,
                         SocietyName = s1.SocietyName,
                         SocietyDescription = s1.SocietyDescription,
                         CreatedByPlayerId= s1.CreatedByPlayerId,
                         CreatedDate = s1.CreatedDate
                     }).ToList();

            return new ObservableCollection<Society>( lstPlayerSocieties
                .OrderBy(x => x.SocietyName) );
     
            //return await tableSociety
            //    .OrderBy(x => x.SocietyName)
            //    .Where(x => x.CreatedByPlayerId == playerId)
            //    .ToCollectionAsync();
        }

        public async Task LoadSocietyLookup(string playerId)
        {
            //Load societies to picker lists
           
            ObservableCollection<Society> lstSocieties;
            Debug.WriteLine("Loading Society lookup list...");
            lstSocieties = await GetSocieties(playerId);
            Pickers.PickerSociety = lstSocieties.ToList();
            Lookups._dictSocieties = lstSocieties.ToDictionary(x => x.SocietyId, x => x.SocietyName);
  
        }

        public async Task SaveSocietyAsync(Society society)
        {
            await Initialize();

            try
            {

                if (society.Id == null)
                {
                    await tableSociety.InsertAsync(society);
                }
                else
                {
                    await tableSociety.UpdateAsync(society);
                }
                // await Syncsocieties();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save society: " + ex);
            }
        }
        #endregion Society

        #region SocietyPlayer

        public async Task SaveSocietyPlayerAsync(SocietyPlayer societyPlayer)
        {
            await Initialize();

            try
            {

                if (societyPlayer.Id == null)
                {
                    await tableSocietyPlayer.InsertAsync(societyPlayer);
                }
                else
                {
                    await tableSocietyPlayer.UpdateAsync(societyPlayer);
                }
                // await Syncsocieties();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save society player: " + ex);
            }
        }

        public async Task DeleteSocietyPlayerAsync(SocietyPlayer societyPlayer)
        {
            if (societyPlayer.Id == null) return;

            await Initialize();
            try
            {
                await tableSocietyPlayer.DeleteAsync(societyPlayer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete society Player: " + ex);
            }
        }

        public async Task<ObservableCollection<SocietyPlayer>> GetSocietyPlayers()
        {
            await Initialize();
            return await tableSocietyPlayer
                .ToCollectionAsync();
        }

        public async Task<ObservableCollection<SocietyPlayer>> GetSocietyPlayers(string societyId)
        {
            await Initialize();
            return await tableSocietyPlayer
                .Where(x => x.SocietyId == societyId)
                .ToCollectionAsync();
        }


        #endregion SocietyPlayer

        #region Competition


        public async Task SyncCompetitions()
        {
            try
            {
                //if ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode))
                //    return;
                if ((Connectivity.NetworkAccess == NetworkAccess.None) || (Preferences.Get("OfflineMode", false) == true))
                    return;

                await client.SyncContext.PushAsync();
                _queryId = (_UseQueryId ? "allCompetitions" : null);
                await tableCompetition.PullAsync(_queryId, tableCompetition.CreateQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, using offline capabilities: " + ex.Message);
            }

        }

        public async Task<ObservableCollection<Competition>> GetCompetitions()
        {
            await Initialize();
            await SyncCompetitions();
            return await tableCompetition
                .OrderBy(x => x.StartDate)
                .ToCollectionAsync();
            
        }
        public async Task<ObservableCollection<Competition>> GetCompetitionsForSociety(string societyId)
        {
            await Initialize();
            await SyncCompetitions();
            return await tableCompetition
                .Where(x => x.SocietyId == societyId)
                .OrderBy(x => x.StartDate)
                .ToCollectionAsync();

        }

        public async Task LoadCompetitionLookup(string societyId)
        {
            //Load comps to picker lists
            ObservableCollection<Competition> lstCompetitions;
            Debug.WriteLine("Loading Competition lookup list...");
            lstCompetitions = await GetCompetitionsForSociety(societyId);
            Pickers.PickerCompetition = lstCompetitions.ToList();
            Lookups._dictCompetitions = lstCompetitions.ToDictionary(x => x.CompetitionId, x => x.CompetitionName);
            //Pickers.PickerCompetition = lstCompetitions.Select(x => new CompetitionLookup { Id = x.Id, CompetitionId = x.CompetitionId, CompetitionName = x.CompetitionName }).ToList();

        }

        public async Task SaveCompetitionAsync(Competition competition)
        {
            await Initialize();

            try
            {

                if (competition.Id == null)
                {
                    await tableCompetition.InsertAsync(competition);
                }
                else
                {
                    await tableCompetition.UpdateAsync(competition);
                }
                await SyncCompetitions();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save competition: " + ex);
            }
        }

        #endregion Competition


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


        // Lookup list for pickers

        public async Task<ObservableCollection<CourseLookup>> GetCourseLookup()
        {
            // if we have already loaded courses to picker lists, just return new collection
            if (Pickers.PickerCourse.Count == 0)
            {
                ObservableCollection<Course> lstCourses; 
                Debug.WriteLine("Loading course lookup list...");
                lstCourses = await GetCourses();
                Lookups._dictCourses = lstCourses.ToDictionary(x => x.CourseId, x => x.CourseName);
                Pickers.PickerCourse = lstCourses.Select(x => new CourseLookup { Id = x.Id, CourseId = x.CourseId, CourseName = x.CourseName }).ToList();
            }
            return new ObservableCollection<CourseLookup>(Pickers.PickerCourse);


        }
        public async Task LoadCourseLookup(bool reload = false)
        {
            // if we have already loaded courses to picker lists, just return new collection
            if (( Pickers.PickerCourse.Count == 0) || (reload==true))
            {
                ObservableCollection<Course> lstCourses;
                Debug.WriteLine("Loading course lookup list...");
                lstCourses = await GetCourses();
                Lookups._dictCourses = lstCourses.ToDictionary(x => x.CourseId, x => x.CourseName);
                Pickers.PickerCourse = lstCourses.Select(x => new CourseLookup { Id = x.Id, CourseId = x.CourseId, CourseName = x.CourseName }).ToList();
            }
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

        public async Task<ObservableCollection<Round>> GetRoundsForCompetition(string competitonId)
        {
            await Initialize();
            await SyncRounds();
            return await tableRound
                .Where(t => t.CompetitionId == competitonId)
                .OrderBy(t => t.RoundDate)
                .ToCollectionAsync();

        }

        public async Task<ObservableCollection<Round>> GetRounds(string courseId)
        {
            await Initialize();
            await SyncRounds();
            return await tableRound
                .Where(t => t.CourseId == courseId)
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

        #region PlayerScore

        public async Task<ObservableCollection<PlayerScore>> GetPlayerScores(string roundId)
        {
            await Initialize();
           
            
                return await tablePlayerScore
                    .Where(t => t.RoundId == roundId)
                    .ToCollectionAsync();
            
            

        }

        public async Task SavePlayerScoreAsync(PlayerScore playerScore)
        {
            await Initialize();

            try
            {

                if (playerScore.Id == null)
                {
                    await tablePlayerScore.InsertAsync(playerScore);
                }
                else
                {
                    await tablePlayerScore.UpdateAsync(playerScore);
                }
 
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save society player: " + ex);
            }
        }

        public async Task DeletePlayerScoreAsync(PlayerScore playerScore)
        {
            if (playerScore.Id == null) return;

            await Initialize();
            try
            {
                await tablePlayerScore.DeleteAsync(playerScore);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete course: " + ex);
            }
        }

        #endregion PlayerScore





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