using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anonym.Isometric
{
    using Util;

    [CustomEditor(typeof(ISOBasis))]
    public class ISOBasisEditor : Editor
    {
        bool bPrefab = false;
        bool bEditOnGroundOffset = false;
        SerializedProperty _spDoNotDestroyAutomatically;
        SerializedProperty _spISOOffest;
        SerializedProperty _spOnGroundObject;
        SerializedProperty _spTransformsForFudge;
        ISOBasis _targetIsoBasis;
        IISOBasis _ISOTarget;
        ISOBasis[] _allIsoBasisCash;

        void OnEnable()
        {
            if (target != null && (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab)))
                return;
            _targetIsoBasis = target as ISOBasis;
            _ISOTarget = _targetIsoBasis.GetComponent<IISOBasis>();
            _spDoNotDestroyAutomatically = serializedObject.FindProperty("bDoNotDestroyAutomatically");
            _spISOOffest = serializedObject.FindProperty("_ISO_Offset");
            _spOnGroundObject = serializedObject.FindProperty("isOnGroundObject");
            _spTransformsForFudge = serializedObject.FindProperty("transforms");
        }

        public override void OnInspectorGUI()
        {	
            if (bPrefab)
            {
                base.DrawDefaultInspector();
                return;
            }

            if (undoredo())
                return;

            serializedObject.Update();
                        
            bool bLocalOnGroundToggle = false, bGlobalUpdate = false, bChangedDepthTransform = false;
            SortingOrder(ref bLocalOnGroundToggle, ref bGlobalUpdate, ref bChangedDepthTransform);

            serializedObject.ApplyModifiedProperties();

            if (bGlobalUpdate)
                IsoMap.Update_All_ISOBasis(ref _allIsoBasisCash);
            else if (bLocalOnGroundToggle)
            {
                _ISOTarget.Update_SortingOrder(true);
                _targetIsoBasis.CheckDepth_Transform();
                SceneView.RepaintAll();
            }
            else if(bChangedDepthTransform)
            {
                if (_spOnGroundObject.boolValue)
                    _targetIsoBasis.CheckDepth_Transform();
            }
		}    

        void SortingOrder(ref bool bLocalOnGroundToggle, ref bool bGlobalChanged, ref bool bChangedDepthTransform)
        {
            _spDoNotDestroyAutomatically.boolValue = EditorGUILayout.ToggleLeft(
                new GUIContent("[Tooltip] This Will not automatically destroyed.",
                "If you enable this option, this ISOBasis will not be automatically destroyed" +
                " even if the [Offset for GroundObject setting of IsoMap] is turned off."), 
                _spDoNotDestroyAutomatically.boolValue);

            CustomEditorGUI.DrawSeperator();
            EditorGUI.BeginChangeCheck();
            _spISOOffest.vector3Value = Util.CustomEditorGUI.Vector3Slider(
                _spISOOffest.vector3Value, Vector3.zero,
                "SortingOrder Basis", -0.5f * Vector3.one, 0.5f * Vector3.one, EditorGUIUtility.currentViewWidth);
            bLocalOnGroundToggle |= EditorGUI.EndChangeCheck();

            CustomEditorGUI.DrawSeperator();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _spOnGroundObject.boolValue = EditorGUILayout.ToggleLeft("IsOnGroundObject", _spOnGroundObject.boolValue);
                bLocalOnGroundToggle |= EditorGUI.EndChangeCheck();
                if (_spOnGroundObject.boolValue)
                    bEditOnGroundOffset = EditorGUILayout.Toggle("Edit Global Offset", bEditOnGroundOffset);
            }

            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
            {
                EditorGUILayout.HelpBox("This only works in Auto ISO mode.", MessageType.Info);
                return;
            }

            if (_spISOOffest == null)
                return;

            float fOnGroundOffset = IsoMap.instance.fOnGroundOffset;

            if (_ISOTarget == null)
            {
                Debug.LogWarning("ISOBasis must be located in the game object where the RC or ISO components reside.");
                return;
            }

            if (_spOnGroundObject.boolValue)
            {
                Util.CustomEditorGUI.NewParagraph("[Tooltip], What is OnGroundOffset? ", 
                    "Give them a global offset so that things on the ground are not covered by the edge of tile image of the ground.\n" +
                    "This has the effect of changing the screen depth of the Sprite object. In particular, it affects the SO calculation in AutoIso mode.");
                string desc = string.Format("New SortingOrder is {0}: Modified from {1}", _ISOTarget.CalcSortingOrder(true), _ISOTarget.CalcSortingOrder(false));
                EditorGUILayout.LabelField(desc);

                if (_targetIsoBasis.Parent != null)
                    EditorGUILayout.ObjectField("Parent ISOBasis", _targetIsoBasis.Parent, typeof(ISOBasis), allowSceneObjects: true);
                else
                {
                    if (bEditOnGroundOffset)
                    {
                        EditorGUI.BeginChangeCheck();
                        fOnGroundOffset = Util.CustomEditorGUI.FloatSlider("Global Offset",
                            fOnGroundOffset, 0, 1, EditorGUIUtility.currentViewWidth);
                        CustomEditorGUI.Button(true, CustomEditorGUI.Color_LightBlue, 
                            string.Format("Reset ({0})", IsoMap.fOnGroundOffset_Default),
                            () => fOnGroundOffset = IsoMap.fOnGroundOffset_Default,
                            GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
                        bGlobalChanged |= EditorGUI.EndChangeCheck();
                        if (bGlobalChanged)
                        {
                            Undo.RecordObject(IsoMap.instance, "ISOBasis: Edit Ground Offset");
                            IsoMap.instance.fOnGroundOffset = fOnGroundOffset;
                        }
                    }
                    EditorGUILayout.Space();
                    bChangedDepthTransform = CustomTransform_ForFudge();
                    CustomEditorGUI.DrawSeperator();
                }
            }
            return;
        }

        bool CustomTransform_ForFudge()
        {
            GUIContent label = new GUIContent(string.Format("Transforms({0}) with Auto Depth", _spTransformsForFudge.arraySize),
                "Transforms registered here automatically apply Depth as much as Global Ground Offset. " +
                "Excluding it from the list restores the origin transform.");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_spTransformsForFudge, label, true);
            if (_spTransformsForFudge.isExpanded)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Auto Setup"))
                    {
                        serializedObject.ApplyModifiedProperties();
                        _targetIsoBasis.AutoSetup_DepthTransforms();
                        serializedObject.Update();
                    }

                    CustomEditorGUI.Button(true, Color.gray, "Clear Depthed Transforms", () =>
                    {
                        _spTransformsForFudge.ClearArray();
                    });
                }
            }
            return EditorGUI.EndChangeCheck();
        }

        bool undoredo()
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                IsoMap.Update_All_ISOBasis(ref _allIsoBasisCash);
                Repaint();
                return true;
            }
            return false;
        }
    }
}
