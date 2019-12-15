using System.Linq;
using UnityEngine;

namespace Anonym.Isometric
{
    public static class ColliderHelper
    {
        public static Bounds GetStatelessBounds(this BoxCollider boxCollider)
        {
            if (boxCollider == null)
                return new Bounds();

            if (boxCollider.enabled)
                return boxCollider.bounds;

            var transform = boxCollider.transform;
            var bounds = new Bounds(transform.TransformPoint(boxCollider.center), Vector3.zero);
            bounds.Encapsulate(transform.TransformPoint(boxCollider.center - boxCollider.size * 0.5f));
            bounds.Encapsulate(transform.TransformPoint(boxCollider.center + boxCollider.size * 0.5f));
            return bounds;
        }

        public static Collider fAboveGround(this Collider passCollider, Vector3 position, ref float fOut, float _fMaxHeight = 10f)
        {
            var _hits = Physics.RaycastAll(position, Vector3.down, _fMaxHeight, -1, QueryTriggerInteraction.Collide).OrderBy(r => r.distance).GetEnumerator();

            while(_hits.MoveNext())
            {
                var _hit = (RaycastHit) _hits.Current;
                if (passCollider == null || _hit.collider.gameObject != passCollider.gameObject)
                {
                    fOut = Mathf.Max(fOut, _hit.distance);
                    return _hit.collider;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public static float Collider_DropToFloor(this Collider _col, GameObject _go = null, float _fMaxHeight = 10f, bool bDontMove = false)
        {
            if (_col == null)
                return 0;

            if (_go == null)
                _go = _col.transform.root.gameObject;

            Bounds _bound = _col.bounds;
            Vector3[] vPoints = new Vector3[]{  new Vector3(_bound.center.x, _bound.min.y, _bound.center.z),
                new Vector3(_bound.min.x, _bound.min.y, _bound.min.z),  new Vector3(_bound.min.x, _bound.min.y, _bound.max.z),
                new Vector3(_bound.max.x, _bound.min.y, _bound.min.z),  new Vector3(_bound.max.x, _bound.min.y, _bound.max.z)};

            float fMax = 0;
            float fDistanceToTriggerBox = 0;
            foreach (var position in vPoints)
            {
                if (!_col.fAboveGround(position, ref fMax, _fMaxHeight))
                {
                    IsoTile tile = _go.GetComponentInParent<IsoTile>();
                    if (tile != null)
                    {
                        Bounds _tile_bounds = tile.GetBounds_SideOnly();
                        fDistanceToTriggerBox = position.y - _tile_bounds.max.y;
                    }
                }
            }

            if (fMax == 0)
                fMax = fDistanceToTriggerBox;

            if (!bDontMove && fMax != 0)
            {
                UnityEditor.Undo.RecordObject(_go.transform, "Drop to flop!");
                _go.transform.Translate(0, -fMax, 0);
            }
            return fMax;
        }
#endif
    }
}