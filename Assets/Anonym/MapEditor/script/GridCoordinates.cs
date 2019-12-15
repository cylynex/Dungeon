using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;
#endif

namespace Anonym.Isometric
{
    using Util;
    [DisallowMultipleComponent]
    [System.Serializable]
    [ExecuteInEditMode][DefaultExecutionOrder(1)]
    public class GridCoordinates : MonoBehaviour
    {
        [SerializeField]
        Grid _grid;
        public Grid grid
        {
            get
            {
                if (_grid == null)
                {
                    GridReset();
                }
                return _grid;
            }
        }
        public void GridReset()
        {
            if (GetComponent<IsoTileBulk>() == null)
                _grid = GetComponent<Grid>();

            Transform _parent = transform.parent;
            while (_grid == null && _parent != null)
            {
                _grid = _parent.GetComponent<Grid>();
                _parent = _parent.parent;
            }

            if (_grid == null)
            {
                _grid = IsoMap.instance.gGrid;
            }
        }

#if UNITY_EDITOR
        public bool bSnapFree = false;

        [SerializeField]
        AutoNaming _autoName;

        [HideInInspector]
        public AutoNaming autoName
        {
            get
            {
                return _autoName == null ?
                    _autoName = GetComponent<AutoNaming>() : _autoName;
            }
        }

        void _reset()
        {
            _grid = null;
            UpdateXYZ();
        }

        private void OnEnable()
        {
            _reset();
        }
        void OnTransformParentChanged()
        {
            _reset();
        }

        public bool bChangedforEditor = false;
        bool bIgnoreTransformChanged = false;

        void Update()
        {         
			if (!Application.isEditor || Application.isPlaying || !enabled)
                // ||  gameObject.transform.root == gameObject.transform)
				return;
			
            if (transform.hasChanged)
            {
                if (bIgnoreTransformChanged == true)
                    bIgnoreTransformChanged = false;
                else
                    Update_TransformChanged();
            }
		}

        void LateUpdate()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
            }
        }

        public void Update_TransformChanged()
        {
            Apply_SnapToGrid();
            //UpdateXYZ();
            bChangedforEditor = true;
        }

		
        [HideInInspector]
        Vector3 _lastLocalPosition;
        void update_LastLocalPosition()
        {
            if (_lastLocalPosition != transform.localPosition)
            {
                _lastLocalPosition = transform.localPosition;
                Rename();
            }
        }
		public void Apply_SnapToGrid()
		{
            if (bSnapFree)
                return;// + grid.Centor;
                
            UpdateXYZ();
            Vector3 v3Delta = transform.localPosition;

            v3Delta -= Vector3.Scale(_lastXYZ, grid.GridInterval);

			if (v3Delta != Vector3.zero)
			{                
			    UnityEditor.Undo.RecordObject(transform, "IsoTile:Move");
				transform.localPosition -= v3Delta;
				update_LastLocalPosition();
			}
		}
        public bool IsSame(Vector3 _ref_coordinates, bool X = true, bool Y = true, bool Z = true)
        {
            return (!X || _ref_coordinates.x.Equals(_lastXYZ.x))
                &&  (!Y || _ref_coordinates.y.Equals(_lastXYZ.y))
                && (!Z || _ref_coordinates.z.Equals(_lastXYZ.z));
        }
        public void Update_Grid(bool _bIgnoreTransformChanged)
        {
            Vector3 _NewPos = Vector3.Scale(grid.GridInterval, _lastXYZ);
			if (_NewPos != Vector3.zero)
			{                
			    UnityEditor.Undo.RecordObject(transform, "IsoTile:Move");
				transform.localPosition = _NewPos;
				update_LastLocalPosition();
                bIgnoreTransformChanged = _bIgnoreTransformChanged;
			}
        }
        Vector3 _lastXYZ;
        public Vector3 _xyz{ get{
            return _lastXYZ; }
        }//xyz(transform.localPosition);} }
        public void UpdateXYZ()
        {
            _lastXYZ = grid.PositionToCoordinates(transform.localPosition, !bSnapFree);
            update_LastLocalPosition();
        }
        public void Translate(Vector3 _coord, string _undoName = "Coordinates:Move")
        {
            Translate(Mathf.RoundToInt(_coord.x), Mathf.RoundToInt(_coord.y), Mathf.RoundToInt(_coord.z), _undoName);
        }

        public void Translate(int _x, int _y, int _z, string _undoName = "Coordinates:Move")
        {
            Undo.RecordObject(transform, _undoName);
            gameObject.transform.localPosition += 
                new Vector3(grid.GridInterval.x * _x, grid.GridInterval.y * _y, grid.GridInterval.z * _z);
            Undo.RecordObject(gameObject, _undoName);
            UpdateXYZ();
        }

        public void Move(Vector3 _coord, string _undoName = "Coordinates:Move")
        {
            Move(_coord.x, _coord.y, _coord.z, _undoName);
        }

        public void Move(float _x, float _y, float _z, string _undoName = "Coordinates:Move")
        {
            Move(Mathf.RoundToInt(_x), Mathf.RoundToInt(_y), Mathf.RoundToInt(_z), _undoName);
        }

        public void Move(int _x, int _y, int _z, string _undoName = "Coordinates:Move")
        {
            Undo.RecordObject(transform, _undoName);
            gameObject.transform.localPosition = 
                new Vector3(grid.GridInterval.x * _x, grid.GridInterval.y * _y, grid.GridInterval.z * _z);
            Undo.RecordObject(gameObject, _undoName);
            UpdateXYZ();            
        }

        public void Rename()
		{
			if (autoName != null)
				autoName.AutoName();
		}

        public void Sync(GridCoordinates with)
        {
            bSnapFree = with.bSnapFree;
            _reset();
        }
#endif
    }
}