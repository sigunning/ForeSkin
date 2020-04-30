using System.Reflection;
using Xamarin.Forms;

namespace ForeScore.Controls

{
    public class FontAwesomeLabel : Label
    {
        public FontAwesomeLabel()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    FontFamily = null;
                    break;
                case Device.Android:
                    FontFamily = "FontAwesome";
                    break;
                case Device.UWP:
                    FontFamily = "/Assets/Fonts/fontawesome-webfont.ttf#FontAwesome";
                    break;
            }
        }
    }

    public class FontAwesomeButton : Button
    {
        public FontAwesomeButton()
        {
            switch(Device.RuntimePlatform)
            {
                case Device.iOS:
                    FontFamily = null;
                    break;
                case Device.Android:
                    FontFamily = "FontAwesome";
                    break;
                case Device.UWP:
                    FontFamily = "/Assets/Fonts/fontawesome-webfont.ttf#FontAwesome";
                    break;
            }
            
        }
    }

}