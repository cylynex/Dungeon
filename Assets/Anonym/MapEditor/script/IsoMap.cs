using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{
	using Util;

	[DisallowMultipleComponent]
	[RequireComponent(typeof(Grid))]
	public class IsoMap : Singleton<IsoMap> {
		public static float fResolution = 100f;		
        public static float fOnGroundOffset_Default = 0.5f;
		public static Vector3 vMAXResolution = Vector3.one * fResolution;

        public static float fCurrentOnGroundOffset
        {
            get
            {
                if (IsNull || !instance.bUseGroundObjectOffset || instance.bUseIsometricSorting)
                    return 0;
                return instance.fOnGroundOffset;
            }
        }

        public Vector3 fResolutionOfIsometric = vMAXResolution;


		public bool bUseIsometricSorting = true;
        public bool bUseGroundObjectOffset = false;

        public float fOnGroundOffset = fOnGroundOffset_Default;
        public Vector3 VOnGroundOffset { get { return Vector3.up * fOnGroundOffset; } }

        public float ReferencePPU = 128;

        [SerializeField]
        Grid _grid;
        public Grid gGrid
        {
            get
            {
                if (_grid == null)
                    _grid = GetComponent<Grid>();
                return _grid;
            }
        }

        [SerializeField]
        public Vector2 TileAngle = new Vector2(30f, -45f);

#if UNITY_EDITOR

        List<IsoTileBulk> _childBulkList = new List<IsoTileBulk>();
		public void Regist_Bulk(IsoTileBulk _add)
		{
			if (_add == null || PrefabUtility.GetPrefabType(_add).Equals(PrefabType.Prefab))
				return;

			if (!_childBulkList.Exists(r => r == _add))
			{
				_childBulkList.Add(_add);
			}
		}
		public void Update_Grid()
		{
			for(int i = _childBulkList.Count - 1; i >= 0 ; --i)
			{
				if(_childBulkList[i] == null)
				{
					_childBulkList.RemoveAt(i);
					continue;
				}
				_childBulkList[i].coordinates.Update_Grid(true);
				if (_childBulkList[i].coordinates.grid.IsInheritGrid)
				{
					_childBulkList[i].Update_Grid();
				}
			}
		}
		
		float _last_TileAngle_Y = 0;
		float _last_Scale_TA_Y = 1f;
		public float fScale_TA_Y(Vector3 _v3Size)
		{
			bool bCosRange = (TileAngle.y >= -45f && TileAngle.y < 45f)
					|| (TileAngle.y >= 135f && TileAngle.y < 225f);
			if (_last_TileAngle_Y != TileAngle.y)
			{
				_last_TileAngle_Y = TileAngle.y;
				if (bCosRange)
					_last_Scale_TA_Y = Mathf.Cos(Mathf.Deg2Rad * _last_TileAngle_Y);
				else
					_last_Scale_TA_Y = Mathf.Sin(Mathf.Deg2Rad * _last_TileAngle_Y);
			}
			return Mathf.Abs((bCosRange ? _v3Size.x : _v3Size.z) / _last_Scale_TA_Y);
		}
		private Vector2 _lastTileAngle = Vector2.zero;
		private float _lastMagicValue = 2f;
		public float fMagicValue{
			get{
				if (TileAngle.Equals(_lastTileAngle))
					return _lastMagicValue;
				return _lastMagicValue = Mathf.Abs(2f * ((3 * Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * TileAngle.x), 2) - 1) 
					/ (3 * Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * TileAngle.y), 2) - 1) + 1) / 3f);
			}
		}
        public Plane GetIsometricPlane(Vector3 normal, Vector3 inPoint)
        {
            return new Plane(Quaternion.Euler(TileAngle) * normal, inPoint);
        }
        public Plane GetIsometricPlane(Vector3 inPoint)
        {
            return new Plane(Quaternion.Euler(TileAngle) * Vector3.back, inPoint);
        }
        public Plane GetIsometricGroundPlane(Vector3 inPoint)
        {
            return new Plane(Quaternion.Euler( -TileAngle.x, TileAngle.y, 0f) * Vector3.up, inPoint);
        }

        [SerializeField]
		public Camera GameCamera;
		
		[SerializeField]
		bool bCustomResolution = false;
		[SerializeField]
		Vector3 vCustomResolution = vMAXResolution;

        // new Vector3(85.1f, 10.3f, 52.5f);
        public IsometricSortingOrder[] Revert_All_ISO()
        {
            IsometricSortingOrder[] _ISOArray = FindObjectsOfType<IsometricSortingOrder>();
            if (_ISOArray != null)
            {
                for (int i = 0; i < _ISOArray.Length; ++i)
                {
                    if (_ISOArray[i] != null)
                    {
                        _ISOArray[i].Revert_SortingOrder();
                    }
                }
            }
            return _ISOArray;
        }
        public IsometricSortingOrder[] Backup_All_ISO()
        {
            IsometricSortingOrder[] _ISOArray = FindObjectsOfType<IsometricSortingOrder>();
            if (_ISOArray != null)
            {
                for (int i = 0; i < _ISOArray.Length; ++i)
                {
                    if (_ISOArray[i] != null)
                    {
                        _ISOArray[i].Backup_SortingOrder();
                    }
                }
            }
            return _ISOArray;
        }
		public void UpdateIsometricSortingResolution()
		{
			if (bUseIsometricSorting)
			{
				if (!bCustomResolution)
				{
					fResolutionOfIsometric.Set(
						Mathf.Max(Grid.fGridTolerance, Mathf.Sin(Mathf.Deg2Rad * -TileAngle.y) * fResolution),
						Mathf.Max(Grid.fGridTolerance, Mathf.Sin(Mathf.Deg2Rad * TileAngle.x) * fResolution),
						Mathf.Max(Grid.fGridTolerance, Mathf.Cos(Mathf.Deg2Rad * -TileAngle.y) * fResolution)
					);
				}
				else
				{
					fResolutionOfIsometric = vCustomResolution;
				}
			}
			else
				fResolutionOfIsometric.Set(0f, 0f, 0f);
		}

        public void Update_All_ISO()
        {
            Update_All_ISO(FindObjectsOfType<IsometricSortingOrder>());
        }
        public void Clear_All_ISO_Backup()
        {
            var all = FindObjectsOfType<IsometricSortingOrder>();
            foreach(var one in all)
                one.Clear_Backup();
        }
        public void Update_All_ISO(IsometricSortingOrder[] _ISOArray)
		{
            if (_ISOArray == null)
                return;

			for (int i = 0 ; i < _ISOArray.Length; ++i)
			{
				if (_ISOArray[i] != null)
				{
					_ISOArray[i].Update_SortingOrder(true);
				}
			}
		}
        public static void Update_All_ISOBasis()
        {
            ISOBasis[] _allIsoBasisCash = null;
            Update_All_ISOBasis(ref _allIsoBasisCash);
        }
        public static void Update_All_ISOBasis(ref ISOBasis[] _allIsoBasisCash)
        {
            if (_allIsoBasisCash == null || _allIsoBasisCash.Length == 0)
                _allIsoBasisCash = FindObjectsOfType<ISOBasis>().Where(r => r.isOnGroundObject).ToArray();

            if (!(_allIsoBasisCash == null || _allIsoBasisCash.Length == 0))
            {
                foreach (var one in _allIsoBasisCash)
                    one.Update_SortingOrder();
                SceneView.RepaintAll();
            }
        }
		public void Update_TileAngle()
		{
			UpdateIsometricSortingResolution();

			if (SceneView.lastActiveSceneView != null)
			{
                if (SceneView.lastActiveSceneView.in2DMode)
                {
                    SceneView.lastActiveSceneView.in2DMode = false;
                    SceneView.lastActiveSceneView.orthographic = true;
                }
                else if (SceneView.lastActiveSceneView.orthographic == false)
					SceneView.lastActiveSceneView.orthographic = true;

				SceneView.lastActiveSceneView.LookAtDirect(
					IsoMap.instance.transform.position, 
					Quaternion.Euler(TileAngle));
			}

			if (GameCamera != null)
			{
				GameCamera.transform.rotation = Quaternion.Euler(TileAngle);
				if (GameCamera.orthographic == false)
				{
					GameCamera.orthographic = true;
					GameCamera.orthographicSize = ((GameCamera.pixelHeight)/(1f * ReferencePPU)) * 0.5f;
				}

			}
		}
		
		public GameObject BulkPrefab;
		public GameObject TilePrefab;
		public GameObject OverlayPrefab;
		public GameObject TriggerPlanePrefab;
		public GameObject TriggerCubePrefab;
		public GameObject ObstaclePrefab;
        public GameObject Side_Union_Prefab;
		public GameObject Side_X_Prefab;
		public GameObject Side_Y_Prefab;
		public GameObject Side_Z_Prefab;
		public GameObject Collider_X_Prefab;
		public GameObject Collider_Y_Prefab;
		public GameObject Collider_Z_Prefab;
		public GameObject Collider_Cube_Prefab;
        public GameObject TchPrefab;
		public Sprite IsoTile_Union_OutlineImage;
		public Sprite IsoTile_Side_OutlineImage;
		public Sprite RefTileSprite;

        public GameObject GetSidePrefab(Iso2DObject.Type _type)
		{
			switch(_type)
			{
				case Iso2DObject.Type.Side_Union:
					return Side_Union_Prefab;
				case Iso2DObject.Type.Side_X:
					return Side_X_Prefab;
				case Iso2DObject.Type.Side_Y:
					return Side_Y_Prefab;
				case Iso2DObject.Type.Side_Z:
					return Side_Z_Prefab;
			}
			return null;
		}

		public IsoTileBulk NewBulk()
		{			
			if (BulkPrefab == null)
			{
				Debug.LogError("IsoMap : No BulkPrefab!");
				return null;
			}
			IsoTileBulk _newBulk = GameObject.Instantiate(BulkPrefab).GetComponent<IsoTileBulk>();
			Undo.RegisterCreatedObjectUndo(_newBulk.gameObject, "IsoTile:Create");
			_newBulk.transform.SetParent(transform, false);
            _newBulk.coordinates.Move(gGrid.Centor);
			return _newBulk;
		}

        public IsoTileBulk NewBulk(IsoTileBulk syncWith, IEnumerable<IsoTile> tiles)
        {
            IsoTileBulk _newBulk = NewBulk();
            _newBulk.bAllowEmptyBulk = true;
            _newBulk.Clear();
            _newBulk.Sync(syncWith);
            var enumerator = tiles.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var current = enumerator.Current;
                Undo.SetTransformParent(current.transform, _newBulk.transform, "IsoTile: Split Bulk");
                current.transform.parent = _newBulk.transform;
            }
            _newBulk.bAllowEmptyBulk = false;
            Selection.activeGameObject = _newBulk.gameObject;
            EditorGUIUtility.PingObject(_newBulk.gameObject);
            return _newBulk;
        }

        public IsoTile NewTile_Raw()
		{
			if (TilePrefab == null)
			{
				Debug.LogError("IsoMap : No TilePrefab!");
				return null;
			}
			IsoTile _newTile = GameObject.Instantiate(TilePrefab).GetComponent<IsoTile>();
			Undo.RegisterCreatedObjectUndo(_newTile.gameObject, "IsoTile:Create");			
			return _newTile;
		}
		IsoTileBulk[] GetAllBulk()
		{
			return gameObject.GetComponentsInChildren<IsoTileBulk>();
		}

		public void BakeNavMesh()
		{
		}

		//void OnValidate()
		//{
		//	if (!PrefabUtility.GetPrefabType(this).Equals(PrefabType.Prefab)
		//		&& Application.isEditor && !Application.isPlaying 
		//		&& !EditorApplication.isPlayingOrWillChangePlaymode
		//		&& !EditorApplication.isUpdating
		//		&& !EditorApplication.isTemporaryProject
		//		&& !IsNull && isActiveAndEnabled)
		//		Update_TileAngle();
		//}

#endif
        public static Vector3 vDepthFudge(float fFudge)
        {
            return Quaternion.Euler(instance.TileAngle) * Vector3.forward * fFudge;
        }
    }
}