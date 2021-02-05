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
        IMobileServiceSyncTable<Round> tableRound;
        IMobileServiceSyncTable<PlayerScore> tablePlayerScore;
   
        
       

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
            store.DefineTable<Society>();
            store.DefineTable<SocietyPlayer>();
            


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
                // increase pull rows  beyond 50  default
                PullOptions pullOptions = new PullOptions
                {
                    MaxPageSize = 500
                };

                // TODO: Introduce Where clauses to limit to User related data
                if (SyncOptionsObj.Courses)
                { 
                    await tableCourse.PullAsync(_queryId, tableCourse.CreateQuery(), pullOptions);  
                }
                if (SyncOptionsObj.Players)
                {   
                    await tablePlayer.PullAsync(_queryId, tablePlayer.CreateQuery(), pullOptions);  
                }
                if (SyncOptionsObj.Societies)
                { 
                    await tableSociety.PullAsync(_queryId, tableSociety.CreateQuery(), pullOptions);
                    await tableSocietyPlayer.PullAsync(_queryId, tableSocietyPlayer.CreateQuery(), pullOptions);
                    //await tableSocietyPlayer.PullAsync(_queryId, tableSocietyPlayer.CreateQuery().Where(u => u.PlayerId == Preferences.Get("PlayerId",null)), pullOptions);
                }
                if (SyncOptionsObj.Competitions)
                {
                    await tableCompetition.PullAsync(_queryId, tableCompetition.CreateQuery(), pullOptions);
                    await tableRound.PullAsync(_queryId, tableRound.CreateQuery(), pullOptions);
                }
                if (SyncOptionsObj.Scores)
                {
                    
                    await tablePlayerScore.PullAsync(_queryId, tablePlayerScore.CreateQuery(), pullOptions );
                }

                
            

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
            //await SyncPlayers();
            ObservableCollection<Player> results = await tablePlayer
                .Where(x => x.PlayerId == playerId)
                .ToCollectionAsync();
            return results.SingleOrDefault();
            
               
        }

        public async Task<Player> GetPlayerByUserId(string userId)
        {
            await Initialize();
            //await SyncPlayers();
            ObservableCollection<Player> results = await tablePlayer
                .Where(x => x.userId == userId)
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
            

            List<Society> societies = await tableSociety.ToListAsync();
            List<SocietyPlayer>  societyPlayers = await tableSocietyPlayer
                .Where(o => o.PlayerId == playerId).ToListAsync();

            List<Society> lstPlayerSocieties = (from s1 in societies
                                                join sp1 in societyPlayers on s1.SocietyId equals sp1.SocietyId
                                                select s1).ToList();

            /*
            ObservableCollection<Society> societies = await GetSocieties();
            ObservableCollection<SocietyPlayer> societyPlayers = await GetSocietyPlayers();
            List <Society> lstPlayerSocieties = new List<Society>();
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
            */

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

        public async Task<Society> GetHomeSociety(string playerId)
        {
            await Initialize();
            List<Society> lstSocieties = await tableSociety
                .Where(o => o.CreatedByPlayerId == playerId).ToListAsync();
            return lstSocieties.FirstOrDefault();   

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

        public async Task<Competition> GetCompetition(string competitionId)
        {
            await Initialize();

            ObservableCollection<Competition> lstCompetitions;
            lstCompetitions = await tableCompetition
                .Where(o => o.CompetitionId == competitionId)
                .ToCollectionAsync();
            Competition competition = lstCompetitions.FirstOrDefault(o => o.CompetitionId == competitionId);

            return competition;

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
            //await SyncCourses();
           
            return await tableCourse
                .OrderBy(x=> x.CourseName)
                .ToCollectionAsync();

        }

        public async Task<Course> GetCourse(string courseId)
        {
            await Initialize();
            //await SyncCourses();
            ObservableCollection<Course> lstCourses;
            lstCourses = await tableCourse
                .Where(o => o.CourseId == courseId)
                .ToCollectionAsync() ;
            Course course = lstCourses.FirstOrDefault(o => o.CourseId == courseId);
            // populate course Par & SI array lists
            StoreCourseParSI(course);


            return course;

        }

        private void StoreCourseParSI(Course course)
        {
            // populate array of Par
            course.arPar = new int[19];
            course.arPar[0] = course.PAR;
            course.arPar[1] = course.H1_Par;
            course.arPar[2] = course.H2_Par;
            course.arPar[3] = course.H3_Par;
            course.arPar[4] = course.H4_Par;
            course.arPar[5] = course.H5_Par;
            course.arPar[6] = course.H6_Par;
            course.arPar[7] = course.H7_Par;
            course.arPar[8] = course.H8_Par;
            course.arPar[9] = course.H9_Par;
            course.arPar[10] = course.H10_Par;
            course.arPar[11] = course.H11_Par;
            course.arPar[12] = course.H12_Par;
            course.arPar[13] = course.H13_Par;
            course.arPar[14] = course.H14_Par;
            course.arPar[15] = course.H15_Par;
            course.arPar[16] = course.H16_Par;
            course.arPar[17] = course.H17_Par;
            course.arPar[18] = course.H18_Par;
            // etc TODO
            // populate array of stroke index
            course.arSI = new int[19];
            course.arSI[0] = course.SSS;
            course.arSI[1] = course.H1_SI;
            course.arSI[2] = course.H2_SI;
            course.arSI[3] = course.H3_SI;
            course.arSI[4] = course.H4_SI;
            course.arSI[5] = course.H5_SI;
            course.arSI[6] = course.H6_SI;
            course.arSI[7] = course.H7_SI;
            course.arSI[8] = course.H8_SI;
            course.arSI[9] = course.H9_SI;
            course.arSI[10] = course.H10_SI;
            course.arSI[11] = course.H11_SI;
            course.arSI[12] = course.H12_SI;
            course.arSI[13] = course.H13_SI;
            course.arSI[14] = course.H14_SI;
            course.arSI[15] = course.H15_SI;
            course.arSI[16] = course.H16_SI;
            course.arSI[17] = course.H17_SI;
            course.arSI[18] = course.H18_SI;
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

        public async Task<Round> GetRound(string roundId)
        {
            await Initialize();
            ObservableCollection<Round> lstRounds;
            lstRounds = await tableRound
                .Where(o => o.RoundId == roundId)
                .ToCollectionAsync();
            Round round = lstRounds.FirstOrDefault(o => o.RoundId == roundId);
      
            return round;

        }

        public async Task<ObservableCollection<Round>> GetRoundsForCompetition(string competitonId)
        {
            await Initialize();
            
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

        public async Task <PlayerScore> GetPlayerScore(string roundId, string playerId)
        {
            await Initialize();

            ObservableCollection<PlayerScore> lstPlayerScores;
            lstPlayerScores= await tablePlayerScore
                .Where(t => t.RoundId == roundId && t.PlayerId ==playerId)
                .ToCollectionAsync();
            return lstPlayerScores.FirstOrDefault();
        }

        public async Task<ObservableCollection<PlayerScore>> GetPlayerScores(string roundId, string markerId)
        {
            await Initialize();

            return await tablePlayerScore
                .Where(t => t.RoundId == roundId && t.MarkerId == Preferences.Get("PlayerId",null) )
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



    }
}