using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Speech;
using System.Net;
using System.IO;
using System.ServiceModel;
using Android.Speech.Tts;

namespace WatchApp
{
    [Activity(Label = "Instant Eye", MainLauncher = true, Icon = "@drawable/instant_eye")]
    public class NDHomeActivity : Activity
    {
        private static int SPEECH_REQUEST_CODE = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.NDHome);

            // Get our button from the layout resource,
            // and attach an event to it
            Button btnHello = FindViewById<Button>(Resource.Id.HelloButton);
            btnHello.Click += Button_Click;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            //displaySpeechRecognizer();

            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("You don't seem to have a microphone to record with");
              
                alert.Show();
            }
            SpeechRecognitionHelper.run(this);
           // Toast.MakeText(this,"Hello from android app.", ToastLength.Short).Show();
        }

        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == SPEECH_REQUEST_CODE)
            {
                if (resultVal == Result.Ok)
                {
                    Toast.MakeText(this, "Command Accepted", ToastLength.Short).Show();

                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    var textBox = FindViewById<EditText>(Resource.Id.editText);
                    if (matches.Count != 0)
                    {

                        textBox.Text = matches[0];

                        string voiceCommand = textBox.Text.Replace(" ", "");
                        voiceCommand = voiceCommand.ToLower();
                        string speakOut = string.Empty;
                        string message = string.Empty;
                        if (voiceCommand.Contains("eventinfo"))
                        {
                            message = GetEventInformation(textBox.Text);
                            speakOut = "Event Information retrived";
                        }
                        else if (voiceCommand.Contains("personinfo") || voiceCommand.Contains("personal"))
                        {
                            message = GetPersonInformation(textBox.Text);
                            speakOut = "Person information retrived";
                        }
                        else if(voiceCommand.Contains("vehicleinfo") || voiceCommand.Contains("vehicle"))
                        {
                            message = GetVehicleInformation(textBox.Text);
                            speakOut = "Vehicle information retrived";
                        }
                        else if(voiceCommand.Contains("createevent"))
                        {
                            message = "Event E20150000154 created";//CreateEvent(textBox.Text);
                            speakOut = "Event created";
                        }
                        else
                        {
                            speakOut = "Command unsuccessful";
                        }

                        Intent resultintent = new Intent(this, typeof(ResultViewActivity));
                        resultintent.PutExtra("result", message);
                        resultintent.PutExtra("voiceCommand", speakOut);
                        StartActivity(resultintent);                       
                        //  Toast.MakeText(this, "text", ToastLength.Long);               
                    }
                    else
                    {
                        AlertDialog alertDialog = new AlertDialog.Builder(this).Create();
                        alertDialog.SetTitle("Alert");
                        alertDialog.SetMessage("No speech was recognised");
                        // alertDialog.SetButton("Ok", HandllerNotingButton);
                        alertDialog.Show();
                        //Toast.MakeText(this, "No speech was recognised", ToastLength.Long); 
                    }

                    //Toast.MakeText(this, "No speech ", ToastLength.Long);
                }
                //this.OnActivityResult(requestCode, resultVal, data);
            }
            //Toast.MakeText(this, "No requestcode ", ToastLength.Long);
        }


        private string GetVehicleInformation(string text)
        {
            return PingServer(@"http://129.135.50.209/CADRMSWeb/api/CADRMS/GetVehicleInfoFromVehicleId?vehicleId=SCRIPTCAR");
        }

        private string GetPersonInformation(string text)
        {
            return PingServer(@"http://129.135.50.209/CADRMSWeb/api/CADRMS/GetPersonInfo?fname=KIT&lname=TAM&city=boston");
        }

        private string GetEventInformation(string eventId)
        {
            return PingServer(@"http://129.135.50.209/CADRMSWeb/api/CADRMS/GetEventInfomationFromEventId?Id=E20150000064");
        }

      
        private string PingServer(string inputUrl)
        {
            //Spinner spinner = FindViewById<Spinner>(Resource.Id.spinnerSite);
            //string selectedSite = spinner.SelectedItem.ToString();
            //string url = string.Format(inputUrl, selectedSite);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(inputUrl);
            request.ContentType = "application/json";
            request.Method = "GET";

            string message = string.Empty;
            try
            {
                //string message = string.Empty;
                //using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                //using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                //{

                //    message = sr.ReadToEnd();
                //    if (response != null && response.StatusCode == HttpStatusCode.OK)
                //    {
                //        ShowMessage("Status", message);
                //    }
                //    else
                //        ShowMessage("Status", message);

                //}

                using (WebResponse response = request.GetResponse())
                {
                    // Get a stream representation of the HTTP web response:
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        // Use this stream to build a JSON document object:
                        message = stream.ReadToEnd();
                        if (response == null)
                        {
                            ShowMessage("Status", message);
                        }
                        //else
                        //    ShowMessage("Status", message);
                    }
                }
            }
            catch (Exception ex) 
            {
                ShowMessage("Status", ex.Message);
            }

            return message;
        }
      

        private void ShowMessage(string title, string message)
        {
            AlertDialog alertDialog = new AlertDialog.Builder(this).Create();
            alertDialog.SetTitle(title);
            alertDialog.SetMessage(message);
            alertDialog.SetButton("Ok", HandllerNotingButton);
            alertDialog.Show();
        }
        void HandllerNotingButton(object sender, DialogClickEventArgs e)
        {

        }

        //private void displaySpeechRecognizer()
        //{   
        //    string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
        //    if (rec != "android.hardware.microphone")
        //    {
        //        var alert = new AlertDialog.Builder(this);
        //        alert.SetTitle("You don't seem to have a microphone to record with");
        //        alert.SetPositiveButton("OK", (sender, e) =>
        //        {
        //            return;
        //        });
        //        alert.Show();
        //    }
        //    else
        //    {
        //        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        //        //voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 5000);
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
        //        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
        //        StartActivityForResult(voiceIntent, SPEECH_REQUEST_CODE);
        //    }
        //}
    }
}

