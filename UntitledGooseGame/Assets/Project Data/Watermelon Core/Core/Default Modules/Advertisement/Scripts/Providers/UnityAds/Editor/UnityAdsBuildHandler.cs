using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Watermelon
{

    public class UnityAdsBuildHandler : IPreprocessBuildWithReport
    {
        public int callbackOrder => -5;

        public void OnPreprocessBuild(BuildReport report)
        {
#if MODULE_UNITYADS
            AdsData adsData = EditorUtils.GetAsset<AdsData>();

            if(adsData == null)
            {
                Debug.LogError("AdsData don`t exist.");
                return;
            }

            SerializedObject adsDataObject = new SerializedObject(adsData);
            SerializedProperty unityAdsContainerProperty = adsDataObject.FindProperty("unityAdsContainer");
            unityAdsContainerProperty.FindPropertyRelative("androidAppID").stringValue = UnityEditor.Advertisements.AdvertisementSettings.GetGameId(RuntimePlatform.Android);
            unityAdsContainerProperty.FindPropertyRelative("iOSAppID").stringValue = UnityEditor.Advertisements.AdvertisementSettings.GetGameId(RuntimePlatform.IPhonePlayer);
            adsDataObject.FindProperty("testMode").boolValue = UnityEditor.Advertisements.AdvertisementSettings.testMode;
            adsDataObject.ApplyModifiedProperties();
#endif
        }
    }
}
