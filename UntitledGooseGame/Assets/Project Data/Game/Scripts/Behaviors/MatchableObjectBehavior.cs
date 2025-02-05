#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public class MatchableObjectBehavior : MonoBehaviour
    {
        private static readonly int GRAYSCALE_ID = Shader.PropertyToID("_Grayscale");
        private static readonly int TINT_2_ID = Shader.PropertyToID("_Tint2");
        private static readonly int OVERLAY_STRENGTH_ID = Shader.PropertyToID("_OverlayStrength");

        public MatchableObject MatchableObject { get; set; }

        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] ShakeBehavior shakeBehavior;

        public bool IsActive { get; private set; }

        public int LayerId { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public bool IsTransitioning { get; set; }

        public UnityAction nextTransition;

        public void Init(int layerId, int x, int y)
        {
            LayerId = layerId;
            X = x;
            Y = y;
        }

        public void Shake()
        {
            shakeBehavior.Shake();
        }

        public void SetActive(bool isActive, bool withAnimation = true)
        {
            IsActive = isActive;

            for(int i = 0; i < meshRenderer.materials.Length; i++)
            {
                if (withAnimation)
                {
                    meshRenderer.materials[i].DoFloat(OVERLAY_STRENGTH_ID, (isActive ? 0f : 0.9f), 0.5f);
                } else
                {
                    meshRenderer.materials[i].SetFloat(OVERLAY_STRENGTH_ID, (isActive ? 0f : 0.9f));
                }
            }
        }

        public void SwapPositions(MatchableObjectBehavior other)
        {
            int saveLayerId = LayerId;
            LayerId = other.LayerId;
            other.LayerId = saveLayerId;

            int saveX = X;
            X = other.X;
            other.X = saveX;

            int saveY = Y;
            Y = other.Y;
            other.Y = saveY;

            Vector3 savePosition = transform.position;
            transform.position = other.transform.position;
            other.transform.position = savePosition;

            bool saveActive = IsActive;
            IsActive = other.IsActive;
            other.IsActive = saveActive;
        }
    }
}
