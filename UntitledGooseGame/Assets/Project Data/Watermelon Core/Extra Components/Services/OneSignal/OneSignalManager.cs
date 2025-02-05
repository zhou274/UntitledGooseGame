using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    [Define("MODULE_ONESIGNAL")]
    public class OneSignalManager
    {
        public void Init(string appID)
        {
#if MODULE_ONESIGNAL
            // Uncomment this method to enable OneSignal Debugging log output 
            OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.VERBOSE, OneSignal.LOG_LEVEL.NONE);

            // Replace 'YOUR_ONESIGNAL_APP_ID' with your OneSignal App ID.
            OneSignal.StartInit(appID).HandleNotificationOpened(HandleNotificationOpened).Settings(new Dictionary<string, bool>() 
            {
                { OneSignal.kOSSettingsAutoPrompt, false },
                { OneSignal.kOSSettingsInAppLaunchURL, false } 
            }).EndInit();

            OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;

            // The promptForPushNotifications function code will show the iOS push notification prompt. We recommend removing the following code and instead using an In-App Message to prompt for notification permission.
            OneSignal.PromptForPushNotificationsWithUserResponse(OneSignal_promptForPushNotificationsResponse);
#endif
        }

#if MODULE_ONESIGNAL
        private void OneSignal_promptForPushNotificationsResponse(bool accepted)
        {
            Debug.Log("OneSignal_promptForPushNotificationsResponse: " + accepted);
        }

        // Gets called when the player opens the notification.
        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {

        }
#endif
    }
}
