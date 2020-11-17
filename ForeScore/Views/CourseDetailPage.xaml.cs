using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CourseDetailPage : ContentPage
    {
        public CourseDetailPage(Course course)
        {
            InitializeComponent();
            BindingContext = new CourseDetailViewModel();

            // assign passed course item to viewmodel Course
            viewModel.Course = course;

            //Title = viewModel.Course.CourseName;
            Title = viewModel.Course.CourseName?? "Add New Course";

            viewModel.IsNew = ( course.CourseName   == null);

        }

        private CourseDetailViewModel viewModel
        {
            get { return BindingContext as CourseDetailViewModel; }
        }




    }
}