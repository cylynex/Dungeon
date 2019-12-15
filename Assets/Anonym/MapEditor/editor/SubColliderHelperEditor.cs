using UnityEditor;

namespace Anonym.Isometric
{
    using Util;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SubColliderHelper))]
    public class SubColliderHelperEditor : Editor
    {
        bool IsPrefab = false;
        bool bAutoReParent = false;

        private void OnEnable()
        {
            if (IsPrefab = CustomEditorGUI.IsPrefab(targets))
                return;
        }

        public override void OnInspectorGUI()
        {
            if (IsPrefab)
            {
                base.DrawDefaultInspector();
                return;
            }

            CustomEditorGUI.ColliderControlHelperGUI(targets);
        }

        private void OnSceneGUI()
        {
            if (IsPrefab)
                return;
        }

    }
}
