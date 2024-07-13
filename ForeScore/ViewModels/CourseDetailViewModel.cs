using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using ForeScore.Helpers;


namespace ForeScore.ViewModels
{
    class CourseDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        
        public bool IsNew;

        public ICommand SaveCommand { private set; get; }
        public ICommand CourseDataCommand { private set; get; }
        public ICommand FindCIDCommand { private set; get; }

        
        public List<int> ParList { get; set; }
        public List<int> SIList { get; set; }

        // constructor
        public CourseDetailViewModel()
        {
            //Title = Course.CourseName?? "Add New Course";
            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SaveCommand = new Command( async()  =>
            {
                IsBusy = true;
                if (IsNew)
                {
                    MessagingCenter.Send(_course, "AddNew");
                }
                else
                { 
                    MessagingCenter.Send(_course, "Update"); 
                }

                Debug.WriteLine("Course saved: ");
                await Shell.Current.Navigation.PopAsync();
                IsBusy = false;

            });

            // launch web page to find course
            FindCIDCommand = new Command( async () =>
            {
                IsBusy = true;
                await Shell.Current.Navigation.PushAsync(new WebViewPage());

                IsBusy = false;

            });

            

            // set up PAR picker list
            ParList = Common.Pickers.PickerPar.ToList();
            //ParList = new List<int>(new int[]  { 3, 4, 5 });
            // set up SI picker list
            //int[] sequence = Enumerable.Range(1, 18).ToArray();
            SIList = Common.Pickers.Picker18.ToList();
            //SIList = new List<int>(sequence);

            // subscribe to messaging so that we can receive id from webview
            SubscribeToMessageCenter();


        }

        private void SubscribeToMessageCenter()
        {
            MessagingCenter.Subscribe<String>(this, "mscorecard", (cid) =>
            {
                // set course data id
                IsBusy = true;
                LoadCourseData(cid);
                IsBusy = false;
            });

        }



        private bool LoadCourseData(string mscorecardId)
        {
            HtmlScrape ht = new HtmlScrape();

            string url = @"https://www.mscorecard.com/mscorecard/showcourse.php?cid=" + mscorecardId.ToString();

            try
            {
                int[,] aCourseData = ht.GetTable(url);

               

                // update course slots
                _course.H1_Par = aCourseData[0, 1];
                _course.H2_Par = aCourseData[1, 1];
                _course.H3_Par = aCourseData[2, 1];
                _course.H4_Par = aCourseData[3, 1];
                _course.H5_Par = aCourseData[4, 1];
                _course.H6_Par = aCourseData[5, 1];
                _course.H7_Par = aCourseData[6, 1];
                _course.H8_Par = aCourseData[7, 1];
                _course.H9_Par = aCourseData[8, 1];
                _course.H10_Par = aCourseData[9, 1];
                _course.H11_Par = aCourseData[10, 1];
                _course.H12_Par = aCourseData[11, 1];
                _course.H13_Par = aCourseData[12, 1];
                _course.H14_Par = aCourseData[13, 1];
                _course.H15_Par = aCourseData[14, 1];
                _course.H16_Par = aCourseData[15, 1];
                _course.H17_Par = aCourseData[16, 1];
                _course.H18_Par = aCourseData[17, 1];

                // update SI
                _course.H1_SI = aCourseData[0, 2];
                _course.H2_SI = aCourseData[1, 2];
                _course.H3_SI = aCourseData[2, 2];
                _course.H4_SI = aCourseData[3, 2];
                _course.H5_SI = aCourseData[4, 2];
                _course.H6_SI = aCourseData[5, 2];
                _course.H7_SI = aCourseData[6, 2];
                _course.H8_SI = aCourseData[7, 2];
                _course.H9_SI = aCourseData[8, 2];
                _course.H10_SI = aCourseData[9, 2];
                _course.H11_SI = aCourseData[10, 2];
                _course.H12_SI = aCourseData[11, 2];
                _course.H13_SI = aCourseData[12, 2];
                _course.H14_SI = aCourseData[13, 2];
                _course.H15_SI = aCourseData[14, 2];
                _course.H16_SI = aCourseData[15, 2];
                _course.H17_SI = aCourseData[16, 2];
                _course.H18_SI = aCourseData[17, 2];


                Course = _course;

                return true;
            }
            catch
            {
                // 
                return false;
            }

        }

        private Course _course;
        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;
                OnPropertyChanged();
            }
        }

       
    }

   
}
