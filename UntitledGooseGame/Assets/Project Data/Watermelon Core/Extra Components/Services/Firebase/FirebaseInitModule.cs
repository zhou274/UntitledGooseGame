using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/Firebase Manager")]
    public class FirebaseInitModule : InitModule
    {
        public override void CreateComponent(Initialiser Initialiser)
        {
            FirebaseManager firebaseManager = new FirebaseManager();
            firebaseManager.Init();
        }

        public FirebaseInitModule()
        {
            moduleName = "Firebase Manager";
        }
    }
}
