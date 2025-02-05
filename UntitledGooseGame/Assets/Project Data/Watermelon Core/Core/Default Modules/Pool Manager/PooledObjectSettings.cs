using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PooledObjectSettings
    {
        //activate
        private bool activate;
        private bool useActiveOnHierarchy;
        public bool Activate => activate;
        public bool UseActiveOnHierarchy => useActiveOnHierarchy;

        //position
        private Vector3 position;
        private bool applyPosition;
        public Vector3 Position => position;
        public bool ApplyPosition => applyPosition;

        //localPosition
        private Vector3 localPosition;
        private bool applyLocalPosition;
        public Vector3 LocalPosition => localPosition;
        public bool ApplyLocalPosition => applyLocalPosition;

        //eulerRotation
        private Vector3 eulerRotation;
        private bool applyEulerRotation;
        public Vector3 EulerRotation => eulerRotation;
        public bool ApplyEulerRotation => applyEulerRotation;

        //localEulerRotation
        private Vector3 localEulerRotation;
        private bool applyLocalEulerRotatition;
        public Vector3 LocalEulerRotation => localEulerRotation;
        public bool ApplyLocalEulerRotatition => applyLocalEulerRotatition;

        //localRotation
        private Quaternion localRotation;
        private bool applyLocalRotatition;
        public Quaternion LocalRotation => localRotation;
        public bool ApplyLocalRotatition => applyLocalRotatition;

        //localScale
        private Vector3 localScale;
        private bool applyLocalScale;
        public Vector3 LocalScale => localScale;
        public bool ApplyLocalScale => applyLocalScale;

        //parrent
        private Transform parrent;
        private bool applyParrent;
        public Transform Parrent => parrent;
        public bool ApplyParrent => applyParrent;



        public PooledObjectSettings(bool activate = true, bool useActiveOnHierarchy = false)
        {
            this.activate = activate;
            this.useActiveOnHierarchy = useActiveOnHierarchy;

            applyPosition = false;
            applyEulerRotation = false;
            applyLocalEulerRotatition = false;
            applyLocalRotatition = false;
            applyLocalScale = false;
            applyParrent = false;
        }

        public PooledObjectSettings SetActivate(bool activate)
        {
            this.activate = activate;
            return this;
        }

        public PooledObjectSettings SetPosition(Vector3 position)
        {
            this.position = position;
            applyPosition = true;
            return this;
        }

        public PooledObjectSettings SetLocalPosition(Vector3 localPosition)
        {
            this.localPosition = localPosition;
            applyLocalPosition = true;
            return this;
        }

        public PooledObjectSettings SetEulerRotation(Vector3 eulerRotation)
        {
            this.eulerRotation = eulerRotation;
            applyEulerRotation = true;
            return this;
        }

        public PooledObjectSettings SetLocalEulerRotation(Vector3 eulerRotation)
        {
            this.localEulerRotation = eulerRotation;
            applyLocalEulerRotatition = true;
            return this;
        }

        public PooledObjectSettings SetLocalRotation(Quaternion rotation)
        {
            this.localRotation = rotation;
            applyLocalRotatition = true;
            return this;
        }

        public PooledObjectSettings SetLocalScale(Vector3 localScale)
        {
            this.localScale = localScale;
            applyLocalScale = true;
            return this;
        }

        public PooledObjectSettings SetLocalScale(float localScale)
        {
            this.localScale = localScale * Vector3.one;
            applyLocalScale = true;
            return this;
        }

        public PooledObjectSettings SetParrent(Transform parrent)
        {
            this.parrent = parrent;
            applyParrent = true;
            return this;
        }
    }
}

// -----------------
// IAP Manager v 1.6.4
// -----------------