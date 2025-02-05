using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ShakeBehavior : MonoBehaviour
    {
        private Vector3 savePositon;
        private bool isShaking = false;
        private float startTime;
        private float duration;

        public void Shake()
        {
            savePositon = transform.position;
            startTime = Time.time;
            isShaking = true;
            duration = 1;
        }

        private void FixedUpdate()
        {
            if (isShaking)
            {
                if(Time.time - startTime >= duration)
                {
                    isShaking = false;

                    transform.position = savePositon;

                    return;
                }

                transform.position = savePositon + Random.onUnitSphere * 0.025f;
            }
        }
    }
}