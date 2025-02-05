using UnityEngine;
#if MODULE_FIREBASE && MODULE_FIREBASE_DYNAMIC_LINKS
using Firebase.DynamicLinks;
#endif

namespace Watermelon
{
    [Define("MODULE_FIREBASE_DYNAMIC_LINKS")]
    public class FirebaseDynamicLinks
    {
        public void Init()
        {
#if MODULE_FIREBASE && MODULE_FIREBASE_DYNAMIC_LINKS
            if (FirebaseManager.IsInitialised)
            {
                DynamicLinks.DynamicLinkReceived += OnDynamicLink;
            }
            else
            {
                FirebaseManager.OnFirebaseInitialised += WaitForFirebaseInit;
            }
#else
            Debug.Log("[Firebase]: Define \"MODULE_FIREBASE_DYNAMIC_LINKS\" or \"MODULE_FIREBASE\" disabled.");
#endif
        }

#if MODULE_FIREBASE && MODULE_FIREBASE_DYNAMIC_LINKS
        private void OnDynamicLink(object sender, System.EventArgs args)
        {
            var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;

            Debug.LogFormat("[Firebase]: Received dynamic link {0}", dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
        }

        private void WaitForFirebaseInit()
        {
            FirebaseManager.OnFirebaseInitialised -= WaitForFirebaseInit;

            Init();
        }
#endif
    }
}
