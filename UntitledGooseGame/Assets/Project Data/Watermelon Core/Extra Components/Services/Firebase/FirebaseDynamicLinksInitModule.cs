using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/Firebase Dynamic Links")]
    public class FirebaseDynamicLinksInitModule : InitModule
    {
        public FirebaseDynamicLinksInitModule()
        {
            moduleName = "Firebase Dynamic Links";
        }

        public override void CreateComponent(Initialiser Initialiser)
        {
            FirebaseDynamicLinks firebaseDynamicLinks = new FirebaseDynamicLinks();
            firebaseDynamicLinks.Init();
        }
    }
}
