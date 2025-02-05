using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/Facebook Manager")]
    public class FacebookInitModule : InitModule
    {
        [SerializeField] string appID;

        public override void CreateComponent(Initialiser Initialiser)
        {
            FacebookManager facebookManager = Initialiser.gameObject.AddComponent<FacebookManager>();
            facebookManager.Init(appID);
        }

        public FacebookInitModule()
        {
            moduleName = "Facebook Manager";
        }
    }
}
