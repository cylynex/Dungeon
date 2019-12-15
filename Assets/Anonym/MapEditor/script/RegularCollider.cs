using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Anonym.Isometric
{
	[DisallowMultipleComponent]
    [ExecuteInEditMode][DefaultExecutionOrder(3)]
    public class RegularCollider : SubColliderHelper, IISOBasis
    {
        #region Basic
        Iso2DObject[] _iso2DsCash = null;
        public Iso2DObject[] Iso2Ds{get{
			return _iso2DsCash != null ?
				_iso2DsCash : (_iso2DsCash = findIso2DObject());
		}}
        Iso2DObject[] findIso2DObject()
        {
            List<Iso2DObject> _iso2ds = new List<Iso2DObject>();
            Iso2DObject _iso2d;
            for (int i = 0; i < transform.childCount; ++i)
            {
                if ((_iso2d = transform.GetChild(i).GetComponent<Iso2DObject>()) != null)
                    _iso2ds.Add(_iso2d);
            }
            return _iso2ds.Count > 0 ? _iso2ds.ToArray() : null;
        }

        IsoTile _tile = null;
		public IsoTile Tile{get{return _tile != null ? _tile : (_tile = GetComponentInParent<IsoTile>());}}

        public Bounds GetBounds()
        {
            if (BC != null)
                return BC.GetStatelessBounds();
            return new Bounds(transform.position, Tile.coordinates.grid.TileSize);
        }
        public RegularCollider ParentRC
        {
            get
            {
                if (transform.parent == null)
                    return null;
                return transform.parent.GetComponent<RegularCollider>();
            }
        }

        #endregion Basic

        #region SortingOrder
        // The following items must be not UNITY_EDITOR for dynamic generation:
        public void Update_SortingOrder(bool bChildRC = false)
        {
            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            if (Iso2Ds != null)
            {
                int _so = CalcSortingOrder();
                for (int i = 0; i < _iso2DsCash.Length; ++i)
                {
                    _so = _iso2DsCash[i].Update_SortingOrder(_so);
                }
            }

            if (bChildRC)
            {
                RegularCollider[] _rcs = GetComponentsInChildren<RegularCollider>();
                if (_rcs != null && _rcs.Length > 0)
                {
                    foreach (var _obj in _rcs)
                    {
                        if (_obj != null && _obj != this)
                        {
                            _obj.Update_SortingOrder(true);
                        }
                    }
                }
            }
        }
        public int CalcSortingOrder(bool bWithBasis = true)
        {
            if (bWithBasis && _ISOBasis != null && _ISOBasis.bActivated)
                return _ISOBasis.CalcSortingOrder();
            return IsometricSortingOrderUtility.IsometricSortingOrder(GetBounds().center);
        }
        #endregion SortingORder

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
            UnityEditor.Editor.DestroyImmediate(_ISOBasis);
#else
            Destroy(this);
#endif
            Remove();
        }

        public bool IsOnGroundObject()
        {
            if (Tile.gameObject == gameObject)
                return false;
            var list = Iso2Ds;
            return list != null && list.Length > 0 && list[0] != null && list[0].IsColliderAttachment;
        }
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (_iso2DsCash != null && _iso2DsCash.Length == 0)
                _iso2DsCash = null;
        }

        void Update()
        {
            if (!enabled || IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return;

            if (transform.hasChanged)
            {
                Update_SortingOrder();
                transform.hasChanged = false;
            }
        }
        void OnTransformParentChanged()
        {
            _tile = null;
            Update_SortingOrder();
        }
        void OnTransformChildrenChanged()
        {
            _iso2DsCash = null;
#if UNITY_EDITOR
            _subColliders = null;
#endif
            Update_SortingOrder();
        }
        #endregion

#if UNITY_EDITOR
        #region MapEditor
        [SerializeField]
        List<int> ISOListForBackup = new List<int>();


        [SerializeField]
		Vector3 _vIso2DScaleMultiplier = Vector3.one;
		public Vector3 Iso2DScaleMultiplier {get{return _vIso2DScaleMultiplier;}}		

		SubColliderHelper[] _subColliders;
		public SubColliderHelper[] SubColliders{get{
			if (_subColliders == null)
				update_subColliders();
			return _subColliders; 
		}}
        public void update_subColliders()
        {
            List<SubColliderHelper> _tmpList = new List<SubColliderHelper>();
            for (int i = 0 ; i < transform.childCount; ++i)
            {
                SubColliderHelper _sub = transform.GetChild(i).GetComponent<SubColliderHelper>();
                if (_sub != null)
                    _tmpList.Add(_sub);
            }
			if (_tmpList.Count > 0)
            	_subColliders = _tmpList.Where(r => !(r is RegularCollider)).ToArray();
        }
        public void ResetSortingOrder(int iNewSortingOrder)
        {
            ISOListForBackup.Clear();
            if (Iso2Ds != null)
            {
                for (int i = 0; i < _iso2DsCash.Length; ++i)
                {
                    _iso2DsCash[i].Update_SortingOrder(iNewSortingOrder);
                }
            }
        }
        public void Backup_SortingOrder()
        {
            ISOListForBackup.Clear();
            if (Iso2Ds != null)
            {
                for(int i = 0; i < Iso2Ds.Length; ++i)
                {
                    ISOListForBackup.Add(Iso2Ds[i].sprr.sortingOrder);
                }
            }
        }
        public void Revert_SortingOrder()
        {
            if (IsoMap.IsNull || IsoMap.instance.bUseIsometricSorting)
                return;

            if (Iso2Ds != null)
            {
                for (int i = 0; i < _iso2DsCash.Length; ++i)
                {
                    _iso2DsCash[i].Update_SortingOrder(ISOListForBackup.Count > i ? ISOListForBackup[i] : 0);
                }
            }
            ISOListForBackup.Clear();
        }
                
		public override void Toggle_UseGridTileScale(bool bTBackup_FRestore)
		{
			base.Toggle_UseGridTileScale(bTBackup_FRestore);

			RegularCollider[] _rcs = GetComponentsInChildren<RegularCollider>();
			if (_rcs != null && _rcs.Length > 0)
			{
				foreach(var _obj in _rcs)
				{
					if (_obj != null && _obj != this)
					{
						_obj.Toggle_UseGridTileScale(bTBackup_FRestore);
					}
				}
			}

			if (SubColliders != null && SubColliders.Length > 0)
			{
				foreach(var _obj in SubColliders)
				{
					if (_obj != null)
					{
						_obj.Toggle_UseGridTileScale(bTBackup_FRestore);
					}
				}
			}

			AdjustScale();
		}		
		public void AdjustScale()
		{
			if (Tile.bAutoFit_ColliderScale)
			{
				Vector3 _tileSize = Tile.coordinates.grid.TileSize;
				ScaleMultiplier(_tileSize);
				if (SubColliders != null && SubColliders.Length > 0)
				{
					foreach(var _obj in SubColliders)
					{
						if (_obj != null)
						{
							_obj.ScaleMultiplier(_tileSize);
						}
					}
				}
			}
		}
        #endregion MapEditor
#endif        
    }
}