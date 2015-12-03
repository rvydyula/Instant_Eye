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
using Android.Speech;
using Android.Content.PM;

namespace WatchApp
{
    /**
 * A helper class for speech recognition
 */
    public class SpeechRecognitionHelper
    {
        private static readonly int VOICE;

        /**
             * Running the recognition process. Checks availability of recognition Activity,
             * If Activity is absent, send user to Google Play to install Google Voice Search.
            * If Activity is available, send Intent for running.
             *
             * @param callingActivity = Activity, that initializing recognition process
             */
        public static void run(Activity callingActivity)
        {
            // check if there is recognition Activity
            if (isSpeechRecognitionActivityPresented(callingActivity) == true)
            {
                // if yes – running recognition
                startRecognitionActivity(callingActivity);
            }
            else
            {
                // if no, then showing notification to install Voice Search
                Toast.MakeText(callingActivity, "In order to activate speech recognition you must install 'Google Voice Search'", ToastLength.Long).Show();
                // start installing process
                //InstallGoogleVoiceSearch(callingActivity);
            }
        }

        /**
     * Checks availability of speech recognizing Activity
     *
     * @param callerActivity – Activity that called the checking
     * @return true – if Activity there available, false – if Activity is absent
     */
        private static Boolean isSpeechRecognitionActivityPresented(Activity callerActivity)
        {
            try
            {
                // getting an instance of package manager
                PackageManager pm = callerActivity.PackageManager;
                // a list of activities, which can process speech recognition Intent
                var activities = pm.QueryIntentActivities(new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);

                if (activities.Any())
                {    // if list not empty
                    return true; // then we can recognize the speech
                }
            }
            catch (Exception e)
            {

            }

            return false; // we have no activities to recognize the speech
        }

        /**
    * Send an Intent with request on speech 
    * @param callerActivity  - Activity, that initiated a request
    */
        private static void startRecognitionActivity(Activity callerActivity)
        {

            //// creating an Intent with “RecognizerIntent.ACTION_RECOGNIZE_SPEECH” action
            //Intent intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);

            //// giving additional parameters:
            //intent.PutExtra(RecognizerIntent.ExtraPrompt, "Start speaking the command.");    // user hint
            //intent.PutExtra(RecognizerIntent.ActionRecognizeSpeech,true);    // setting recognition model, optimized for short phrases – search queries
            //intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            //// quantity of results we want to receive
            ////choosing only 1st -  the most relevant 
            //var data = intent.Data;
            //// start Activity ant waiting the result
            //callerActivity.StartActivityForResult(intent,0);
          

            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults,1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);

            callerActivity.StartActivityForResult(voiceIntent, 1);


        }


    }
}