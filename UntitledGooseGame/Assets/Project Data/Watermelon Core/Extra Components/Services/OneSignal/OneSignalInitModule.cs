using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/OneSignal Manager")]
    public class OneSignalInitModule : InitModule
    {
        [SerializeField] string iOSAppID;
        [SerializeField] string androidAppID;

        public override void CreateComponent(Initialiser Initialiser)
        {
            OneSignalManager oneSignalManager = new OneSignalManager();
            oneSignalManager.Init(GetAppID());
        }

        public OneSignalInitModule()
        {
            moduleName = "OneSignal Manager";
        }

        public string GetAppID()
        {
#if UNITY_EDITOR
            return string.Empty;
#elif UNITY_ANDROID
            return androidAppID;
#elif UNITY_IOS
            return iOSAppID;
#else
            return "unexpected_platform";
#endif
        }
    }
}
