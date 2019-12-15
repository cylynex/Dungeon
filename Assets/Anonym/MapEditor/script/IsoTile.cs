using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{	
	using Util;

	public enum SelectionType
	{
		LastTile,
		NewTile,
		AllTile,
	}

    [System.Serializable]
    public class AttachedIso2D : Attachment<Iso2DObject> { }
    [System.Serializable]
    public class AttachedIso2Ds : AttachmentHierarchy<AttachedIso2D> { }

    [SelectionBase]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(GridCoordinates))]
	[RequireComponent(typeof(IsometricSortingOrder))]
	[RequireComponent(typeof(RegularCollider))]
	[ExecuteInEditMode]
    public class IsoTile : MonoBehaviour
    {
        #region Basic
        [SerializeField]
        GridCoordinates _coordinates = null;
        [HideInInspector]
        public GridCoordinates coordinates
        {
            get
            {
                return _coordinates == null ?
                    _coordinates = GetComponent<GridCoordinates>() : _coordinates;
            }
        }
        #endregion

#if UNITY_EDITOR
        #region MapEditor
        [SerializeField]
		IsoTileBulk _bulk;
		[HideInInspector]
		public IsoTileBulk Bulk{get{
			if (_bulk != null)
				return _bulk;
			if (transform.parent != null)
				return _bulk = transform.parent.GetComponent<IsoTileBulk>();
			return null;
		}}

		[SerializeField]
        AutoNaming _autoName = null;
        [HideInInspector]
        public AutoNaming autoName
        {
            get
            {
                return _autoName == null ?
                    _autoName = GetComponent<AutoNaming>() : _autoName;
            }
        }

		public void Rename()
		{
			autoName.AutoName(); 
		}

		[SerializeField]
        IsometricSortingOrder _so = null;
		[HideInInspector]
        public IsometricSortingOrder sortingOrder{get{
            return _so != null ? _so : _so = GetComponent<IsometricSortingOrder>();
        }}

        public void Update_SortingOrder()
        {
            if (sortingOrder != null)
			{
                sortingOrder.Update_SortingOrder(true);
			}
        }
		
		[HideInInspector, SerializeField]
        public AttachedIso2Ds _attachedList = new AttachedIso2Ds();
		public void Update_AttachmentList()
        { 
			_attachedList.Init(gameObject);   
        }

		[SerializeField]
		public bool bAutoFit_ColliderScale = true;
		[SerializeField]
		public bool bAutoFit_SpriteSize = true;

		public bool IsUnionCube()
		{
			return _attachedList.childList.Exists(r => r.AttachedObj._Type == Iso2DObject.Type.Side_Union);
		}
		public Iso2DObject GetSideObject(Iso2DObject.Type _type)
		{
			if (_attachedList.childList.Exists(r => r.AttachedObj._Type == _type))
				return _attachedList.childList.Find(r => r.AttachedObj._Type == _type).AttachedObj;
			return null;
		}
		public Iso2DObject[] GetSideObjects(params Iso2DObject.Type[] _types)
		{
			if (_types == null || _types.Length == 0)
				_types = new Iso2DObject.Type[]{	
					Iso2DObject.Type.Obstacle, Iso2DObject.Type.Overlay,
					Iso2DObject.Type.Side_Union, Iso2DObject.Type.Side_X,
					Iso2DObject.Type.Side_Y, Iso2DObject.Type.Side_Z,
				};
			Iso2DObject[] results = new Iso2DObject[0];
			_attachedList.childList.ForEach(r =>{
				if (r.AttachedObj != null && ArrayUtility.Contains<Iso2DObject.Type>(_types, r.AttachedObj._Type))
					ArrayUtility.Add<Iso2DObject>(ref results, r.AttachedObj);
			});
			return results;
		}

        #region MonoBehaviour
        void Update()
		{			
			if (!Application.isEditor || Application.isPlaying  || !enabled)
				return;

		}
		void OnEnable()
		{
			Update_AttachmentList();
		}
		void OnTransformParentChanged()
		{
			_bulk = null;
		}
		void OnTransformChildrenChanged()
		{
			if (autoName.bPostfix_Sprite)
				Rename();
			Update_AttachmentList();
		}
        #endregion MonoBehaviour

        public IsoTile Duplicate()
		{
			IsoTile result = GameObject.Instantiate(this);
			result.transform.SetParent(transform.parent, false);
			result.Rename();
			Undo.RegisterCreatedObjectUndo(result.gameObject, "IsoTile:Dulicate");			
			return result;
		}		

		public void Copycat(IsoTile from, bool bCopyChild = true, bool bUndoable = true)
		{
			if (from == this)
				return;
				
			coordinates.Apply_SnapToGrid();

			if (bCopyChild)
			{
				for (int i = transform.childCount - 1; i >= 0 ; --i)
				{
					if (bUndoable)
						Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
					else
						DestroyImmediate(transform.GetChild(i).gameObject);
				}

				foreach (Transform child in from.transform) 
				{
					GameObject _newObj = GameObject.Instantiate(child.gameObject, transform, false);
					if (bUndoable)
						Undo.RegisterCreatedObjectUndo(_newObj, "IsoTile:Copycat");
				}

				
			}
			// Update_AttachmentList();
		}	

		public IsoTile Extrude(Vector3 _direction, bool _bContinuously, bool _withAttachment)
		{
			IsoTile _new = Duplicate();
			if (!_withAttachment)
				_new.Clear_Attachment(false);
			Undo.RegisterCreatedObjectUndo(_new.gameObject, "IsoTile:Extrude");
			_new.coordinates.Translate(_direction, "IsoTile:Extrude");	
			Undo.RecordObject(gameObject, "IsoTile:Extrude");
			return _new;			
		}		

		public void Clear_Attachment(bool bCanUndo)
		{
			Iso2DObject[] _iso2Ds = transform.GetComponentsInChildren<Iso2DObject>();
			for (int i = 0; i < _iso2Ds.Length; ++i)
			{
				Iso2DObject _iso2D = _iso2Ds[i];
				if (_iso2D != null && _iso2D.IsAttachment)
					_iso2D.DestoryGameObject(bCanUndo, false);
			}
		}

		void Clear_SideObject(bool bCanUndo)
        {
			Iso2DObject[] _sideObjects = GetSideObjects(
				Iso2DObject.Type.Side_X, Iso2DObject.Type.Side_Y, 
				Iso2DObject.Type.Side_Z, Iso2DObject.Type.Side_Union);

            for (int i = 0; i < _sideObjects.Length; ++i)
			{
				if (_sideObjects[i] != null)
				{
					_sideObjects[i].DestoryGameObject(bCanUndo, true);
				}
			}
        }

		void Add_SideObject(GameObject _prefab, string _UndoMSG)
		{
			GameObject _obj = GameObject.Instantiate(_prefab, transform, false);
			_obj.transform.SetAsFirstSibling();
			RegularCollider _rc = _obj.GetComponent<RegularCollider>();
			_rc.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
			Undo.RegisterCreatedObjectUndo(_obj, _UndoMSG);
			Update_AttachmentList();
		}

		public void Reset_SideObject(bool _bTrueUnion)
		{
			Clear_SideObject(true);
			Add_SideObject(_bTrueUnion ? 
				IsoMap.instance.Side_Union_Prefab 
				: IsoMap.instance.Side_Y_Prefab, 
				"Change Tile Style");
		}

		public void Toggle_Side(bool _bToggle, Iso2DObject.Type _toggleType)
		{
			Iso2DObject _obj = GetSideObject(_toggleType);
			if (_bToggle)
            {
                if (_obj == null)
                {
					Add_SideObject(IsoMap.instance.GetSidePrefab(_toggleType),
						"Created : " + _toggleType + " Object");
                }
            }
            else
            {
                if (_obj != null)
                {
					_obj.DestoryGameObject(true, true);
                    Update_AttachmentList();
                }
            }
		}

        public bool IsLastTile(Vector3 _direction)
        {
            return Bulk.GetTileList_At(coordinates._xyz, _direction, false, true).Count == 0;
        }

        public IsoTile NextTile(Vector3 _direction)
        {
            List<IsoTile> _tiles = Bulk.GetTileList_At(coordinates._xyz, _direction, false, false);
            return (_tiles.Count > 0) ? _tiles[0] : null;
        }

        public bool IsAccumulatedTile_Coordinates(Vector3 _direction)
        {
			Vector3 _xyz = coordinates._xyz;
            List<IsoTile> _tiles = Bulk.GetTileList_At(_xyz, _direction, false, true);

            int iCheckValue = coordinates.grid.CoordinatesCountInTile(_direction);
			
            iCheckValue *= iCheckValue;
            for(int i = 0 ; i < _tiles.Count ; ++i)
            {
                Vector3 diff = Vector3.Scale(_xyz - _tiles[i].coordinates._xyz, _direction);
                if (Mathf.RoundToInt(diff.sqrMagnitude) < iCheckValue)
                {
                    return true;
                }
            }
            return false;
        }

		public bool IsAccumulatedTile_Collider(Vector3 _direction)
        {
			Vector3 _xyz = coordinates._xyz;
            List<IsoTile> _tiles = Bulk.GetTileList_At(_xyz, _direction, false, true);

			Bounds _bounds = GetBounds();
			// Vector3 _diff = transform.position - _bounds.center;
			// _bounds.SetMinMax(_bounds.min + 2f * _diff, _bounds.max + 2f * _diff);
            for(int i = 0 ; i < _tiles.Count ; ++i)
            {
				if (_tiles[i].GetBounds().Intersects(_bounds))
					return true;
            }
            return false;
        }

        public Bounds GetBounds_SideOnly()
        {
            return GetBounds(Iso2DObject.Type.Side_Union, Iso2DObject.Type.Side_X, Iso2DObject.Type.Side_Y, Iso2DObject.Type.Side_Z);
        }


        public Bounds GetBounds()
		{
			Collider[] _colliders = transform.GetComponentsInChildren<Collider>();
			if (_colliders == null || _colliders.Length == 0)
				return new Bounds(transform.position, Vector3.zero);

			Bounds _bounds = new Bounds(_colliders[0].bounds.center, Vector3.zero);
			for(int i = 0 ; i < _colliders.Length; ++i)
			{
                if (_colliders[i] is BoxCollider)
                    _bounds.Encapsulate((_colliders[i] as BoxCollider).GetStatelessBounds());
                else
                    _bounds.Encapsulate(_colliders[i].bounds);
			}
			_bounds.Expand(Grid.fGridTolerance);
			return _bounds;
		}

        public Bounds GetBounds(params Iso2DObject.Type[] _types)
        {
            Iso2DObject[] _Iso2Ds = GetSideObjects(_types);
            Bounds _bounds = new Bounds(transform.position, Vector3.zero);
            if (_Iso2Ds != null)
            {
                for (int i = 0; i < _Iso2Ds.Length; ++i)
                    _bounds.Encapsulate(_Iso2Ds[i].RC.GlobalBounds);
            }
            return _bounds;
        }
        
        public void MoveToZeroground()
        {
            Vector3 _ZeroGround = coordinates._xyz;
            coordinates.Move(_ZeroGround.x, 0, _ZeroGround.z, "IsoTile:MoveToZeroGround");
        }

		public void Init()
		{
			RegularCollider[] _RCs = GetComponentsInChildren<RegularCollider>();
			Vector3 _tilsSize = coordinates.grid.TileSize;
			foreach(var _RC in _RCs)
			{
				_RC.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
				//_RC.AdjustScale();
			}
		}

		public void Update_Grid()
		{
			coordinates.Update_Grid(true);
			RegularCollider[] _RCs = GetComponentsInChildren<RegularCollider>();
			foreach(var _RC in _RCs)
			{
				_RC.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
				// _RC.AdjustScale();
			}
		}

		public void Update_Attached_Iso2DScale()
		{
			foreach(var _attached in _attachedList.childList)
			{
				Iso2DObject _Iso2D = _attached.AttachedObj;
				if (_Iso2D != null)
				{
					_Iso2D.AdjustScale();
				}
			}
		}

        public void SyncIsoLight(GameObject target)
        {
            var allLightRecivers = target.GetComponentsInChildren<IsoLightReciver>();
            if (allLightRecivers != null && allLightRecivers.Length > 0)
            {
                foreach (var one in allLightRecivers)
                    one.ClearAllLights();
            }

            allLightRecivers = GetComponentsInChildren<IsoLightReciver>().Where(r => !allLightRecivers.Contains(r)).ToArray();
            allLightRecivers.Select(r => r.GetAllLightList());
            List<IsoLight> allLights = new List<IsoLight>();
            foreach (var one in allLightRecivers)
                allLights.AddRange(one.GetAllLightList().Where(r => !allLights.Contains(r)));
            allLights.ForEach(r => r.AddTarget(target, true));
        }
        #endregion MapEditor
#endif
    }
}