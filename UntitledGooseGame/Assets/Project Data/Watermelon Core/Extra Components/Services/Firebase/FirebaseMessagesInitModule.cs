using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Services/Firebase Messages")]
    public class FirebaseMessagesInitModule : InitModule
    {
        public override void CreateComponent(Initialiser Initialiser)
        {
            FirebaseMessages firebaseMessages = new FirebaseMessages();
            firebaseMessages.Init();
        }

        public FirebaseMessagesInitModule()
        {
            moduleName = "Firebase Messages";
        }
    }
}