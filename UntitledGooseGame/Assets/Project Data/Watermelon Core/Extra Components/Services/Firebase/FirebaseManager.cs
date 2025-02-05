using UnityEngine;
#if MODULE_FIREBASE
using Firebase;
using Firebase.Analytics;
#endif

namespace Watermelon
{
    [Define("MODULE_FIREBASE")]
    public class FirebaseManager
    {
        private static bool isInitialised = false;
        public static bool IsInitialised => isInitialised;

        public static OnFirebaseInitialisedCallback OnFirebaseInitialised;

        public void Init()
        {
#if MODULE_FIREBASE
            Debug.Log("[Firebase]: Attempt to initialise");

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[Firebase]: Initialisation canceled.");

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(string.Format("[Firebase]: Initialisation failed with exception {0}", task.Exception));

                    return;
                }

                Debug.Log("[Firebase]: Module initialised!");

                isInitialised = true;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                if (OnFirebaseInitialised != null)
                    OnFirebaseInitialised.Invoke();
            });
#else
            Debug.Log("[Firebase]: Define \"MODULE_FIREBASE\" disabled.");
#endif
        }

        public delegate void OnFirebaseInitialisedCallback();
    }
}
