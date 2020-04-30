using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;

/* 'Icon' is the namespace for my solution. So instead you should include your namespaces, of course. */

using ForeScore.Droid;
using ForeScore.Controls;
using Android.Content;

[assembly: ExportRenderer(typeof(FontAwesomeButton), typeof(FontAwesomeButtonRenderer))]
namespace ForeScore.Droid
{

    class FontAwesomeButtonRenderer : ButtonRenderer
    {
        public FontAwesomeButtonRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
            var label = Control;
            Typeface font;
            try
            {
                font = Typeface.CreateFromAsset(Context.Assets, "Fonts/MaterialIcons-Regular.ttf");
                label.Typeface = font;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TTF file not found. Make sure the Android project contains it at '/Assets/Fonts/fontawesome-webfont.ttf'.");
            }

        }
    }

}