using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
    using Util;

    [System.Serializable]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class IsoTransform : MonoBehaviour
    {
#if UNITY_EDITOR
        public Vector3 localRotation;
        public Vector3 localScale;

        protected float IsometricRotationScale = 1f;

        protected void Update()
        {
            if (!Application.isEditor || Application.isPlaying || !enabled)
                return;

            adjustRotation();
            AdjustScale();

        }

        public void AdjustRotation()
        {
            adjustRotation();
        }

        protected void adjustRotation()
        {
            transform.eulerAngles = localRotation + (Vector3) IsoMap.instance.TileAngle;
        }

        virtual public void AdjustScale()
        {
            adjustScale(Vector3.one);
        }

        virtual protected void adjustScale(Vector3 vMultiplier)
        {
            Vector3 v3Tmp = vMultiplier;

            if (localScale.Equals(Vector3.zero))
            {
                localScale = v3Tmp;
            }
            else
            {
                if (IsometricRotationScale != 0f)
                {
                    transform.localScale = Vector3.Scale(v3Tmp, localScale) * IsometricRotationScale;
                }
                else
                {
                    transform.localScale = localScale;
                }
            }
        }
#endif
    }
}