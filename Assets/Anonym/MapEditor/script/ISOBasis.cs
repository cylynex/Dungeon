using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(IISOBasis))]
    [System.Serializable]
    public class ISOBasis : MonoBehaviour, IGizmoDraw
    {
        public bool bActivated = true;
        public bool bDoNotDestroyAutomatically = false;
        public bool isOnGroundObject = true;

        [SerializeField]
        Vector3 _ISO_Offset = new Vector3(0, -0.5f, 0);

        [SerializeField]
        IISOBasis[] _ISOTarget;
        public IISOBasis[] ISOTarget { get {
                return _ISOTarget != null && _ISOTarget.Length != 0 ? _ISOTarget : _ISOTarget = GetComponents<IISOBasis>();
            }
        }

        [SerializeField]
        Dictionary<Transform, float> _depthedTrasforms = new Dictionary<Transform, float>();
        [SerializeField]
        List<Transform> transforms;

        public ISOBasis Parent
        {
            get
            {
                if (transform.parent == null)
                    return null;
                return transform.parent.GetComponent<ISOBasis>();
            }
        }

#if UNITY_EDITOR


        private void Awake()
        {
            Init();
        }

        void Init()
        {
            foreach (var one in ISOTarget)
                one.SetUp(this);
        }

        private void OnDestroy()
        {
            resetHierarchyDependence();
        }

        void resetHierarchyDependence()
        {
            foreach (var one in ISOTarget)
                one.Remove();
            _ISOTarget = null;
        }        

        protected void OnDrawGizmosSelected()
        {
            GizmoDraw();
        }

        public void GizmoDraw()
        {
            if (!bActivated || IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            // Draw cyan offsetBounds
            Bounds bounds = GetBounds();
            Vector3 vOffset = getSortingOrderBasis(bounds);
            if (Parent == null && isOnGroundObject)
                vOffset -= IsoMap.instance.VOnGroundOffset;

            float fHeight = 0;
            const float fGap = 0.25f;
            Collider _col = GetCollider().fAboveGround(vOffset + Vector3.up * fGap, ref fHeight);
            fHeight -= fGap;
            Vector3 vOnGround = vOffset + fHeight * Vector3.down;

            bool bSelected = UnityEditor.Selection.gameObjects.Contains(gameObject);

            Gizmos.color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, bSelected ? 1 : 0.75f); 
            Gizmos.DrawLine(new Vector3(bounds.min.x, vOffset.y, vOffset.z), new Vector3(bounds.max.x, vOffset.y, vOffset.z));
            Gizmos.DrawLine(new Vector3(vOffset.x, bounds.min.y, vOffset.z), new Vector3(vOffset.x, bounds.max.y, vOffset.z));
            Gizmos.DrawLine(new Vector3(vOffset.x, vOffset.y, bounds.min.z), new Vector3(vOffset.x, vOffset.y, bounds.max.z));
            Gizmos.DrawWireSphere(vOnGround, 0.03f);

            // Draw white Offset via Parent
            Vector3 finalSOOffset = vOffset + GetOffsetViaParentRC();
            Gizmos.color = Color.white;
            Gizmos.DrawLine(vOffset, finalSOOffset);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(vOffset, 0.005f);
            Gizmos.DrawWireSphere(finalSOOffset, 0.015f);

            Gizmos.color = new Color32(255, 70, 0, 200);
            Gizmos.DrawLine(vOffset, vOnGround);
            if (_col)
            {
                bounds = _col.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        public void Update_SortingOrder()
        {
            if (ISOTarget != null)
            {
                foreach (var one in _ISOTarget)
                    one.Update_SortingOrder(true);
            }

            if (IsoMap.instance.bUseGroundObjectOffset)
                ApplyDepth_Transforms();
            else
                RevertDepth_Transforms();
        }
#endif

        Vector3 GetOffsetViaParentRC()
        {
            ISOBasis parent = Parent;

            if (parent == null)
                return isOnGroundObject ? IsoMap.instance.VOnGroundOffset : Vector3.zero;

            Bounds bounds = parent.GetBounds();
            Vector3 vBasis = parent.getSortingOrderBasis(bounds) - bounds.center;
            return vBasis + parent.GetOffsetViaParentRC();
        }

        Collider GetCollider()
        {
            if (ISOTarget != null && ISOTarget.Length > 0)
            {
                if (ISOTarget.Any(r => r is RegularCollider))
                    return (ISOTarget.First(r => r is RegularCollider) as RegularCollider).BC;
            }

            Collider _col = GetComponent<Collider>();
            if (_col != null)
                return _col;

            return null;
        }

        Bounds GetBounds()
        {
            if (ISOTarget == null || ISOTarget.Length == 0)
                return new Bounds(transform.position, Vector3.zero);

            if (ISOTarget.Any(r => r is RegularCollider))
                return ISOTarget.First(r => r is RegularCollider).GetBounds();

            var boundsArray = GetComponents<Collider>().Where(r => !r.isTrigger).Select(r => r.bounds).ToArray();
            if (boundsArray == null || boundsArray.Length == 0)
            {
                boundsArray = ISOTarget.Select(r => r.GetBounds()).ToArray();
            }

            Bounds bounds = boundsArray[0];
            for (int i = 1; i < boundsArray.Length; ++i)
                bounds.Encapsulate(boundsArray[i]);

            return bounds;
        }

        Vector3 getSortingOrderBasis()
        {
            return getSortingOrderBasis(GetBounds());
        }

        Vector3 getSortingOrderBasis(Bounds bounds)
        {
            Vector3 vBasis = bounds.center + Vector3.Scale(_ISO_Offset, bounds.size);
            if (Parent == null && isOnGroundObject)
                vBasis += IsoMap.instance.VOnGroundOffset;
            return vBasis;
        }

        //private void OnValidate()
        //{
        //    Update_SortingOrder();
        //}

        public int CalcSortingOrder()
        {
            return IsometricSortingOrderUtility.IsometricSortingOrder(getSortingOrderBasis() + GetOffsetViaParentRC());
        }

        public void AutoSetup_DepthTransforms()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Depthed Transfer: Auto Setup");
#endif
            var lookups = GetComponentsInChildren<Transform>().Where(r => r.GetComponent<Collider>() == null);
            // 중 차일드가 아닌 것,lookups
            transforms = lookups.Where(r => lookups.All(a => a == r || !r.IsChildOf(a))).ToList();
            CheckDepth_Transform();
        }

        public void CheckDepth_Transform()
        {
            RevertDepth_Transforms(false);
            UpdateDepth_Transforms();
            ApplyDepth_Transforms();
        }

        public void ApplyDepth_Transforms()
        {
            UpdateDepth_Transforms();

            if (transforms == null || transforms.Count == 0)
                return;

            transforms.RemoveAll(r => r == null);
            transforms = transforms.Distinct().ToList();
            var lookups = transforms.Where(r => _depthedTrasforms == null || _depthedTrasforms.Count == 0 || !_depthedTrasforms.ContainsKey(r));
            var enumerator = lookups.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var one = enumerator.Current;
                _depthedTrasforms.Add(one, applyFudgeToTranforms(one));
            }
        }

        public void UpdateDepth_Transforms()
        {
            if (_depthedTrasforms == null || _depthedTrasforms.Count == 0)
                return;

            var fTarget = isOnGroundObject ? IsoMap.instance.fOnGroundOffset : 0;
            var enumerator = _depthedTrasforms.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var one = enumerator.Current;
                applyDepthToTranforms(one.Key, fTarget - one.Value);
            }

            foreach (var key in _depthedTrasforms.Keys.ToList())
                _depthedTrasforms[key] = fTarget;
        }

        public void RevertDepth_Transforms(bool bAllClear = true)
        {
            if (_depthedTrasforms == null || _depthedTrasforms.Count == 0)
                return;

            List<Transform> removeList = new List<Transform>();
            var enumerator = _depthedTrasforms.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var one = enumerator.Current;
                if (!bAllClear && transforms.Contains(one.Key))
                    continue;

                revertDepthedTransforms(one.Key, one.Value);
                removeList.Add(one.Key);
            }

            foreach (var one in removeList)
                _depthedTrasforms.Remove(one);
        }

        static float applyFudgeToTranforms(Transform _t)
        {
            return applyDepthToTranforms(_t, IsoMap.instance.fOnGroundOffset);
        }

        static float applyDepthToTranforms(Transform _t, float fFudge)
        {
            var vDiff = IsoMap.vDepthFudge(fFudge);
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(_t, "DepthedTransform");
#endif
            _t.Translate(-vDiff, Space.World);
            return fFudge;
        }

        static void revertDepthedTransforms(Transform _t, float fFudge)
        {
            var vDiff = IsoMap.vDepthFudge(fFudge);
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(_t, "DepthedTransform");
#endif
            _t.Translate(vDiff, Space.World);

        }
    }
}
