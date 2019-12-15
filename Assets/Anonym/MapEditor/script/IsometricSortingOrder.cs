using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{
    public class IsometricSortingOrderUtility
    {
        public static int IsometricSortingOrder(Transform _transform)
        {
            return IsometricSortingOrder(_transform.position);
        }

        public static int IsometricSortingOrder(Vector3 _position)
        {
            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return 0;
            Vector3 v3Tmp = IsoMap.instance.fResolutionOfIsometric;
            return Mathf.RoundToInt(v3Tmp.x * _position.x + v3Tmp.y * _position.y - v3Tmp.z * _position.z);
        }
    }

    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [DefaultExecutionOrder(2)]
    public class IsometricSortingOrder : MonoBehaviour, IISOBasis
    {
        const int Default_LastSortingOrder = int.MinValue;

        #region Basic
        [SerializeField]
        public bool bStaticISO = false;
        [SerializeField]
        public int iLastSortingOrder = 0;
        public void Corrupt_LastSortingOrder()
        {
            iLastSortingOrder = Default_LastSortingOrder;
        }
        public bool Corrupted_LastSortingOrder()
        {
            return iLastSortingOrder == Default_LastSortingOrder;
        }

        [SerializeField]
        int iParticleSortingAdd = 0;

        List<SpriteRenderer> _dependentList = new List<SpriteRenderer>();
        List<SpriteRenderer> DependentList
        {
            get
            {
                update_Child();
                return _dependentList;
            }
        }

        List<Vector3> _particleLastPositionList = new List<Vector3>();
        List<ParticleSystemRenderer> _particleSystemRendererList = new List<ParticleSystemRenderer>();
        List<ParticleSystemRenderer> ParticleSystemRendererList
        {
            get
            {
                update_Child();
                return _particleSystemRendererList;
            }
        }


        bool bCorrupted = false;
        public void Corrupt_Child(bool bFlag)
        {
            bCorrupted = bFlag;
        }

        ParticleSystem.Particle[] particles = null;
        int setParticleArray(ParticleSystem _ps)
        {
            if (particles == null || particles.Length < _ps.main.maxParticles)
                particles = new ParticleSystem.Particle[_ps.main.maxParticles];
            return _ps.GetParticles(particles);
        }

        void update_Child(bool bJustDoIt = false)
        {
            if (bJustDoIt || bCorrupted)
            {
                _particleSystemRendererList.Clear();
                _particleLastPositionList.Clear();
                ParticleSystemRenderer[] _particlesSystemRenderers = transform.GetComponentsInChildren<ParticleSystemRenderer>();
                if (_particlesSystemRenderers != null)
                {
                    for (int i = 0; i < _particlesSystemRenderers.Length; ++i)
                    {
                        _particleSystemRendererList.Add(_particlesSystemRenderers[i]);
                        _particleLastPositionList.Add(Vector3.zero);
                    }
                }

                _dependentList.Clear();
                _regularColliderList.Clear();

                Iso2DObject _iso2D;
                RegularCollider _rc;

                SpriteRenderer[] _sprrs = transform.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < _sprrs.Length; ++i)
                {
                    if ((_iso2D = _sprrs[i].GetComponent<Iso2DObject>()) != null)
                    {
                        if ((_rc = _iso2D.RC) != null)
                        {
                            if (!_regularColliderList.Exists(r => r == _rc))
                            {
                                _regularColliderList.Add(_rc);
                                continue;
                            }
                        }
                    }
                    _dependentList.Add(_sprrs[i]);
                }
            }
            bCorrupted = false;
        }

        List<RegularCollider> _regularColliderList = new List<RegularCollider>();
        List<RegularCollider> RegularColliderList
        {
            get
            {
                update_Child();
                return _regularColliderList;
            }
        }
        #endregion

        #region SortingOrder
        [SerializeField]
        List<IUpdateSortingOrder> _updateCallBack = new List<IUpdateSortingOrder>();
        public void AddUpdateCallBack(IUpdateSortingOrder add)
        {
            if (!_updateCallBack.Contains(add))
                _updateCallBack.Add(add);
        }
        public void RemoveUpdateCallBack(IUpdateSortingOrder remove)
        {
            if (_updateCallBack.Contains(remove))
                _updateCallBack.Remove(remove);
        }        

        [SerializeField]
        int _iExternAdd = 0;
        public int iExternAdd { set { _iExternAdd = value; } get { return _iExternAdd; } }
        public void Update_Transform_SortingOrder()
        {
            update_SortingOrder();
            _updateCallBack.ForEach(r => r.UpdateSortingOrder());
            transform.hasChanged = false;
        }
        public void Update_SortingOrder(bool bJustDoIt = false)
        {
            update_SortingOrder(bJustDoIt);
            update_particleSortingOrder(bJustDoIt);
        }
        public int CalcSortingOrder(bool bWithBasis = true)
        {
            if (bWithBasis && _ISOBasis != null && _ISOBasis.bActivated)
                return _ISOBasis.CalcSortingOrder();
            return IsometricSortingOrderUtility.IsometricSortingOrder(transform) + _iExternAdd;
        }
        void update_particleSortingOrder(bool bJustDoIt = false)
        {
            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            Vector3 _rendererPosition;
            var RSRList = ParticleSystemRendererList;
            for (int i = 0; i < RSRList.Count; ++i)
            {
                _rendererPosition = RSRList[i].transform.position;
                if (bJustDoIt || _particleLastPositionList[i] != _rendererPosition)
                {
                    _particleLastPositionList[i] = _rendererPosition;
                    RSRList[i].sortingOrder = _iExternAdd + iParticleSortingAdd +
                        IsometricSortingOrderUtility.IsometricSortingOrder(RSRList[i].transform);
                }
            }
        }
        void update_SortingOrder(bool bJustDoIt = false)
        {
            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            bool isLastCalc = false;
            if (!Corrupted_LastSortingOrder())
            {
                Corrupt_LastSortingOrder();
                isLastCalc = true;
            }

            if (IsoMap.instance.bUseIsometricSorting)
            {
                int _iNewSortingOrder = CalcSortingOrder();
                if (bJustDoIt || _iNewSortingOrder != iLastSortingOrder)
                {
                    iLastSortingOrder = _iNewSortingOrder;
                }
            }

            var dependentList = DependentList;

            if (IsoMap.instance.bUseIsometricSorting)
            {
                for (int i = 0; i < dependentList.Count; ++i)
                {
                    dependentList[i].sortingOrder = iLastSortingOrder + i + 1;
                }
            }
            else if (isLastCalc || bJustDoIt)
            {
                for (int i = 0; i < dependentList.Count; ++i)
                {
#if UNITY_EDITOR
                    dependentList[i].sortingOrder = _sprrISOListForBackup.Count > i ? _sprrISOListForBackup[i] : 0;
#else
                    dependentList[i].sortingOrder = 0;
#endif
                }

                var particleSystemRenderList = ParticleSystemRendererList;
                for (int i = 0; i < particleSystemRenderList.Count; ++i)
                {
#if UNITY_EDITOR
                    particleSystemRenderList[i].sortingOrder = _prsISOListForBackup.Count > i ? _prsISOListForBackup[i] : 0;
#else
                    particleSystemRenderList[i].sortingOrder = 0;
#endif
                }
            }

            var regularColliderList = RegularColliderList;
            for (int i = 0; i < regularColliderList.Count; ++i)
            {
                regularColliderList[i].Update_SortingOrder();
            }
        }
        #endregion

        #region ISOBasis
        [SerializeField]
        ISOBasis _ISOBasis = null;
        public ISOBasis GetISOBasis()
        {
            return _ISOBasis;
        }
        public void SetUp(ISOBasis basis = null)
        {
            if (basis == null)
                basis = gameObject.AddComponent<ISOBasis>();
            _ISOBasis = basis;
            Update_SortingOrder(true);
        }
        public void Remove()
        {
            _ISOBasis = null;
            Update_SortingOrder(true);
        }
        public void DestroyISOBasis()
        {
            if (_ISOBasis == null || _ISOBasis.bDoNotDestroyAutomatically)
                return;
#if UNITY_EDITOR
            Editor.DestroyImmediate(_ISOBasis);
#else
            Destroy(this);
#endif
            Remove();
        }
        public Bounds GetBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
                return new Bounds(transform.position, IsoMap.instance.gGrid.TileSize);

            Bounds bounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        public bool IsOnGroundObject()
        {
            // Temporary check
            return GetComponentInParent<IsometricMovement>();
        }
        #endregion

        #region MonoBehaviour
        void OnTransformChildrenChanged()
        {
            update_Child(true);
            Update_SortingOrder(true);
        }
        void OnTransformParentChanged()
        {
            Update_SortingOrder(true);
        }
        void OnEnable()
        {
            update_Child(true);
            Update_SortingOrder(true);
        }
        void Update()
        {
            if (!enabled || IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            if (transform.hasChanged)
            {
                Update_Transform_SortingOrder();
            }
            update_particleSortingOrder();
        }
        #endregion

#if UNITY_EDITOR
        #region MapEditor
        [SerializeField]
        List<int> _sprrISOListForBackup = new List<int>();
        [SerializeField]
        List<int> _prsISOListForBackup = new List<int>();
        public void Clear_Backup()
        {
            Reset_SortingOrder(iLastSortingOrder);
        }
        public void Revert_SortingOrder()
        {
            var regularColliderList = RegularColliderList;
            regularColliderList.ForEach(r => r.Revert_SortingOrder());
        }
        public void Backup_SortingOrder()
        {
            _sprrISOListForBackup.Clear();
            var dependentList = DependentList;
            dependentList.ForEach(r => _sprrISOListForBackup.Add(r != null ? r.sortingOrder : 0));

            _prsISOListForBackup.Clear();
            var particleSystemRenderList = ParticleSystemRendererList;
            particleSystemRenderList.ForEach(r => _prsISOListForBackup.Add(r != null ? r.sortingOrder : 0));

            var regularColliderList = RegularColliderList;
            regularColliderList.ForEach(r => r.Backup_SortingOrder());
        }
        public void Reset_SortingOrder(int iNewSortingOrder)
        {
            iLastSortingOrder = iNewSortingOrder;
            _sprrISOListForBackup.Clear();
            _prsISOListForBackup.Clear();

            var dependentList = DependentList;
            if (IsoMap.instance.bUseIsometricSorting)
            {
                for (int i = 0; i < dependentList.Count; ++i)
                {
                    dependentList[i].sortingOrder = iLastSortingOrder + i;
                }
            }
            else
            {
                for (int i = 0; i < dependentList.Count; ++i)
                {
                    dependentList[i].sortingOrder = iNewSortingOrder;
                }

                var particleSystemRenderList = ParticleSystemRendererList;
                for (int i = 0; i < particleSystemRenderList.Count; ++i)
                {
                    particleSystemRenderList[i].sortingOrder = iNewSortingOrder;
                }
            }

            _regularColliderList.ForEach((r) => r.ResetSortingOrder(iNewSortingOrder));
        }
        #endregion MapEditor
#endif
    }
}