using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Speech.Tts;
//using System.Json;

namespace WatchApp
{
    [Activity(Label = "ResultView")]
    public class ResultViewActivity : Activity, TextToSpeech.IOnInitListener
    {
        public TextToSpeech tts { get; set; }
        public string speakOut { get; set; }

        float pitch = 1, speakRate = 1;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ResultView);
            //Create button click event
            Button btnBack = FindViewById<Button>(Resource.Id.BackButton);
            btnBack.Click += Button_Click;

            speakOut = Intent.GetStringExtra("voiceCommand");
            string text1 = Intent.GetStringExtra("result") ?? "Data not available";
            text1 = text1.Replace("{", ""); 
            text1 = text1.Replace("}", "");
            text1 = text1.Replace(@"\", "");
            text1 = text1.Replace("\"", "");
            text1 = text1.Replace("ArrestRecordCount", "Arrests");
            text1 = text1.Replace("IncidentRecordCount", "Incidents Involved");
            text1 = text1.Replace("DuiRecordCount", "DUI Records");
            text1 = text1.Replace("relatedDUIno", "DUI Id");
            text1 = text1.Replace("DriverName", "Driver Name");
            text1 = text1.Replace("stoppedloc", "DUI Location");
            text1 = text1.Replace("stoppeddatetime", "Date/Time");
            text1 = text1.Replace("DUICount", "DUI Records");
            text1 = text1.Replace("TheftCount", "Theft Records");

            text1 = text1.Replace(":", " : ");

            string[] item = text1.Split(',');

            //var itemResult = JsonValue.Parse(text);

            Toast.MakeText(this, "Processing Completed!", ToastLength.Short).Show();


            //View view1 = new View(this);
            //TextView textView = new TextView(this);
            //Toast toast = new Toast(this);
            //textView.Text = "Processing Completed!";
            //Toast.MakeText(this, textView.Text,ToastLength.Long);          
            //textView.SetTextColor(Android.Graphics.Color.Yellow);
            //view1.SetBackgroundColor(Android.Graphics.Color.Orange);           
            //toast.View = view1;
            //toast.Show();

            ListView listView = FindViewById<ListView>(Resource.Id.resultlistView);
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, item);
            listView.Adapter = adapter;

            //// create text to speech object to use synthesize and speak functions.
            //// first parameter: context
            //// second parameter: object implemeting TextToSpeech.IOnInitListener
            //Toast.MakeText(this, "Command successful!", ToastLength.Long).Show();
            tts = new TextToSpeech(this, this);
            string p = string.Empty;
            tts.Speak("Hello World, You shall not pass", QueueMode.Flush, null, p);


        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            // here you can setup language settings

            if (status.Equals(OperationResult.Success))
            {
               // Toast.MakeText(this, "Text To Speech Succeed!", ToastLength.Long).Show();
                
                SpeakOut(speakOut);
            }
            else
                Toast.MakeText(this, "Text To Speech Fail", ToastLength.Long).Show();
        }

        private void SpeakOut(string text)
        {

            //String text = "Speaking from android phone";
            tts.SetLanguage(Java.Util.Locale.Us);
            tts.SetPitch(pitch);
            tts.SetSpeechRate(speakRate);
            tts.Speak(text, QueueMode.Flush, null, "p");
        }

        private void Button_Click(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.NDHome);
        }
    }

}