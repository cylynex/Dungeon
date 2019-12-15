using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Anonym.Isometric
{
    using Util;
    [CustomEditor(typeof(IsoLight))]
    public class IsoLightEditor : Editor
    {
        const string helpMSG = "First, you must select the Iso2D target that will be affected by IsoLight.\nUse the tools below to easily register and edit Targets!";
        bool bPrefab = false;
        bool bIncludeChild = true;
        bool bLightListFoldout = false;
        bool bTemporaryToggle_Static = false;
        bool bTemporaryToggle_Dynamic = false;

        Vector2 scrollPosition;
        Vector2 scrollPosition2;

        IsoLight light;
        List<IsoLight> lightList = new List<IsoLight>();
        List<IsoLight> MutelightList = new List<IsoLight>();

        LayerMask layerMask = 1;
        GameObject lookupObject;
        SerializedProperty targetList;

        void OnEnable()
        {
            if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab)) 
                return;
            if ((light = (IsoLight)target) == null)
                return;
            
            targetList = serializedObject.FindProperty("targetList");

            MutelightList.Clear();
            lightList.Clear();
            lightList.AddRange(FindObjectsOfType<IsoLight>());

            bTemporaryToggle_Static = bTemporaryToggle_Dynamic = false;
        }

        private void OnDisable()
        {
            unmuteSolo(true);
            unmuteSolo(false);
        }

        public override void OnInspectorGUI()
        {
            if (bPrefab)
            {
                base.DrawDefaultInspector();
                return;
            }
            base.DrawDefaultInspector();

            SetUniquePriority();
            Util();            
        }

        public void SetTarget()
        {            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("[Target Select Helper]", EditorStyles.boldLabel);
            lookupObject = EditorGUILayout.ObjectField(lookupObject, typeof(GameObject), allowSceneObjects: true) as GameObject;
            if (lookupObject != null && PrefabUtility.GetPrefabType(lookupObject).Equals(UnityEditor.PrefabType.Prefab))
            {
                lookupObject = null;
                Debug.Log("Prefab is not allowed. Please select only the GameObject in the scene.");
            }
            if (lookupObject)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bIncludeChild = EditorGUILayout.ToggleLeft("Include Child", bIncludeChild);
                    if (GUILayout.Button("Add"))
                    {
                        light.AddTarget(lookupObject, bIncludeChild);
                    }
                    if (GUILayout.Button("Remove"))
                    {
                        light.RemoveTarget(lookupObject, bIncludeChild);
                    }
                }
            }
            EditorGUILayout.Separator();
            layerMask = CustomEditorGUI.LayerMaskField("Layer Mask for Add/Remove", layerMask);
            GUILayoutOption halfWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(layerMask == 0))
                {
                    if (GUILayout.Button("Add all Masked Iso2DBase", halfWidth))
                    {
                        light.AddTarget_All(layerMask);
                    }
                    if (GUILayout.Button("Clear all Masked layer", halfWidth))
                    {
                        light.RemoveTarget_All(layerMask);
                    }
                }
            }
            EditorGUILayout.Separator();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add all Iso2DBase", halfWidth))
                {
                    light.AddTarget_All(layerMask);
                }
                if (GUILayout.Button("Clear TargetList", halfWidth))
                {
                    light.RemoveTarget_All();
                }
            }
        }
        public void Util()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox(helpMSG, MessageType.Info);
            SetTarget();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("[Temporary Help Features]", EditorStyles.boldLabel);
            SoloToggle();
            updateReciver();
            InstanceList();
        }
        public void InstanceList()
        {
            bLightListFoldout = CustomEditorGUI.ObjectListField(lightList, typeof(IsoLightReciver), "IsoListList", bLightListFoldout, true, ref scrollPosition2);
            targetList.isExpanded = CustomEditorGUI.ObjectListField(light.TargetList, typeof(IsoLightReciver), targetList.name, targetList.isExpanded, true, ref scrollPosition);
        }

        void SetUniquePriority()
        {
            int iStartWith = light.UniquePriority;
            bool bStaticLight = light.bStaticLight;
            var enumerator = lightList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current != null && current != light &&
                    current.bStaticLight == bStaticLight &&
                    iStartWith == current.UniquePriority)
                {
                    Debug.Log(string.Format("[Preoccupied] \"UniquePriority({1})\" by \"{2}\"\n", light.name, iStartWith, current.name));
                    enumerator = lightList.GetEnumerator();
                    iStartWith++;
                }
            }
            if (light.UniquePriority != iStartWith)
            {
                light.UniquePriority = iStartWith;
                Debug.Log(string.Format("[Set] \"{0}.UniquePriority\" to {1}", light.name, iStartWith));
            }
        }

        void SoloToggle()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bTemporaryToggle_Static = soloToggle(light.bStaticLight ?  "Solo(Static)" :  "Mute(Static)", true, bTemporaryToggle_Static);
                bTemporaryToggle_Dynamic = soloToggle(light.bStaticLight ? "Mute(Dynamic)" : "Solo(Dynamic)", false, bTemporaryToggle_Dynamic);
            }
        }
        bool soloToggle(string name, bool bStatic, bool bSolo, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            bSolo = EditorGUILayout.ToggleLeft(name, bSolo, options);
            if (EditorGUI.EndChangeCheck())
            {
                if (!bSolo)
                {
                    unmuteSolo(bStatic);
                }
                else
                {
                    var muteList = lightList.Where(r => r.bStaticLight == bStatic && r.TurnOnOff && r != light);
                    foreach (var _light in muteList)
                    {
                        _light.TurnOnOff = false;
                        MutelightList.Add(_light);
                    }
                }
            }
            return bSolo;
        }
        void unmuteSolo(bool bStatic)
        {
            var muteList = MutelightList.Where(r => r.bStaticLight == bStatic);
            foreach (var _light in muteList)
            {
                if (_light)
                {
                    _light.TurnOnOff = true;
                }
            }
            MutelightList.RemoveAll(r => muteList.Contains(r));
        }
        void updateReciver()
        {
            if (GUILayout.Button("Update Target Color"))
                light.UpdateAllReciver();
        }
    }
}
