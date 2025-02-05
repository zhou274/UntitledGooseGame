using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using System.Text.RegularExpressions;

#if MODULE_FIREBASE
using Firebase;
using Firebase.Analytics;
#endif

public class FirebaseAnalyticsController
{
    private static string FormatParameter(string text)
    {
        return Regex.Replace(text, @"(?<=[a-z])([A-Z])", "_$1").ToLower();
    }

    //copy this method and adjust copies to your needs
    //this variables "example_parater1_name" , "example_parater2_name", "example_event_name" should be constants
    public static void ReportExampleEvent(int exampleParam1, string exampleParam2) 
    {
#if MODULE_FIREBASE
        if (FirebaseManager.IsInitialised)
        {
            Parameter[] parameters = new Parameter[2];
            parameters[0] = new Parameter("example_parater1_name", exampleParam1);
            parameters[1] = new Parameter("example_parater2_name", exampleParam2);
            FirebaseAnalytics.LogEvent("example_event_name", parameters);
        }
        else
        {
            Debug.LogError("FirebaseManager.IsInitialised == false. Event not send.");
        }
#endif
    }

}
