using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ForeScore.Models
{

    // Used in ListViewDemoPage

    public class Person

    {

        public Person()

        {

        }



        public Person(string name, DateTime birthday, Color favoriteColor)

        {

            Name = name;

            Birthday = birthday;

            FavoriteColor = favoriteColor;

        }



        public string Name { set; get; }



        public DateTime Birthday { set; get; }



        public Color FavoriteColor { set; get; }

    };

}
