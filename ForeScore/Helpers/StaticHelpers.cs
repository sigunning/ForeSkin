﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ForeScore.Models;
using System.Linq;

namespace ForeScore.Helpers
{
    static class StaticHelpers
    {

        //static lists of courses for grouping
        public static ObservableCollection<Grouping<string, Course>> CoursesGrouped { get; set; }

        public static ObservableCollection<Course> Courses { get; set; }


        public static ForeScore.LogOn.UserContext UserPlayer { get; set; }

        
    }

    
}
