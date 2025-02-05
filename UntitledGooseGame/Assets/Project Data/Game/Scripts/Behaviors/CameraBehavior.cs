#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CameraBehavior : MonoBehaviour
    {
        private static CameraBehavior instance;

        [SerializeField] ParticleSystem leftConfetti;
        [SerializeField] ParticleSystem rightConfetti;

        private static ParticleSystem LeftConfetti => instance.leftConfetti;
        private static ParticleSystem RightConfetti => instance.rightConfetti;
        private static Vector3 Position { set => instance.transform.position = value; }

        public static Camera MainCamera { get; private set; }
        public static float HalfWidth { get; private set; }
        public static float ItemInSlotScale { get; private set; }

        private void Awake()
        {
            instance = this;

            MainCamera = GetComponent<Camera>();
        }

        public static void Init(int levelSize)
        {
            if (MainMenuBehavior.IsTablet())
            {
                HalfWidth = (levelSize + 5) / 2f;
            } else
            {
                HalfWidth = (levelSize + 1) / 2f;
            }
            
            MainCamera.orthographicSize = HalfWidth / MainCamera.aspect;

            Position = new Vector3(HalfWidth, 10, MainCamera.orthographicSize);

            LeftConfetti.transform.position = new Vector3(0, 0, MainCamera.orthographicSize / 2f);
            RightConfetti.transform.position = new Vector3(HalfWidth * 2, 0, MainCamera.orthographicSize / 2f);

            LeftConfetti.transform.localScale = Vector3.one * MainCamera.orthographicSize / 2;
            RightConfetti.transform.localScale = Vector3.one * MainCamera.orthographicSize / 2;

            ItemInSlotScale = HalfWidth * 2f / 10f;
        }

        public static void ExplodeConfetti()
        {
            LeftConfetti.Play();
            RightConfetti.Play();
        }
        
    }

}