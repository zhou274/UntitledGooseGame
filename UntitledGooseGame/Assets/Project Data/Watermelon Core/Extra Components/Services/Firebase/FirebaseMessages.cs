using UnityEngine;
#if MODULE_FIREBASE && MODULE_FIREBASE_MESSAGES
using Firebase.Messaging;
#endif

namespace Watermelon
{
    [Define("MODULE_FIREBASE_MESSAGES")]
    public class FirebaseMessages
    {
        public void Init()
        {
#if MODULE_FIREBASE && MODULE_FIREBASE_MESSAGES
            if(FirebaseManager.IsInitialised)
            {
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
            }
            else
            {
                FirebaseManager.OnFirebaseInitialised += WaitForFirebaseInit;
            }
#else
            Debug.Log("[Firebase]: Define \"MODULE_FIREBASE_MESSAGES\" or \"MODULE_FIREBASE\" disabled.");
#endif
        }

        private void WaitForFirebaseInit()
        {
            FirebaseManager.OnFirebaseInitialised -= WaitForFirebaseInit;

            Init();
        }

#if MODULE_FIREBASE && MODULE_FIREBASE_MESSAGES
        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("[Firebase]: Received Registration Token: " + e.Message.From);
        }

        private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            Debug.Log("[Firebase]: Received a new message from: " + e.Token);

#if UNITY_ANDROID && MODULE_APPSFLYER
            AppsFlyerSDK.AppsFlyerAndroid.updateServerUninstallToken(e.Token);
#endif
        }
#endif
    }
}
