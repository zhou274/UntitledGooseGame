using UnityEngine;
using System.Collections.Generic;
#if MODULE_FACEBOOK
using Facebook.Unity;
#endif

namespace Watermelon
{
    [Define("MODULE_FACEBOOK")]
    public class FacebookManager : MonoBehaviour
    {
        public void Init(string appID)
        {
#if MODULE_FACEBOOK
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
#endif
        }

#if MODULE_FACEBOOK
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
            }
            else
            {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }
#endif
    }
}
