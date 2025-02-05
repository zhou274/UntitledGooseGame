using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/AppsFlyer Manager")]
    public class AppsFlyerInitModule : InitModule
    {
        [SerializeField] string developerKey;
        [SerializeField] string appleID;

        public override void CreateComponent(Initialiser Initialiser)
        {
            AppsFlyerManager appsFlyerManager = new AppsFlyerManager();
            appsFlyerManager.Init(developerKey, appleID, Initialiser.gameObject);
        }

        public AppsFlyerInitModule()
        {
            moduleName = "Appsflyer Manager";
        }
    }
}