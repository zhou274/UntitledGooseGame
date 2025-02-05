using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        private void Awake()
        {
            Button button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(PlaySound);
            }
        }

        public void PlaySound()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}