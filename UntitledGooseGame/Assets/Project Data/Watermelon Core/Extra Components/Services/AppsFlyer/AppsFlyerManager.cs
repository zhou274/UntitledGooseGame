using UnityEngine;
using System.Collections.Generic;
#if MODULE_APPSFLYER
using AppsFlyerSDK;
#endif

namespace Watermelon
{
    [Define("MODULE_APPSFLYER")]
#if MODULE_APPSFLYER
    public class AppsFlyerManager : IAppsFlyerConversionData
#else
    public class AppsFlyerManager
#endif
    {
        public void Init(string devKey, string appleID, GameObject parentGameObject)
        {
#if MODULE_APPSFLYER
            AppsFlyer.initSDK(devKey, appleID);
            AppsFlyer.startSDK();
#endif
        }

#if MODULE_APPSFLYER
        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }
#endif
    }
}
