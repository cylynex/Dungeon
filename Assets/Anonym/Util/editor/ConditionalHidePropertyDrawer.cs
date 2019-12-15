using UnityEngine;
using UnityEditor;

//Original version of the ConditionalEnumHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -
namespace Anonym.Util
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        float HeaderHeight
        {
            get
            {
                return 2 * GUI.skin.label.lineHeight;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool bEnabled = GetConditionalHideAttributeResult(condHAtt, property);
            if (bEnabled || !condHAtt.HidenInspector)
            {
                Rect positionB = position;
                position.height = HeaderHeight;
                if (!string.IsNullOrEmpty(condHAtt.HeaderString))
                {
                    positionB.y += HeaderHeight;
                    positionB.height -= HeaderHeight;
                    EditorGUI.HelpBox(position, condHAtt.HeaderString, MessageType.Info);
                }
                bool bGUIEnabled = GUI.enabled;
                GUI.enabled = bEnabled;
                EditorGUI.PropertyField(positionB, property, label, true);
                GUI.enabled = bGUIEnabled;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
 
			if (!GetConditionalHideAttributeResult(condHAtt, property) && condHAtt.HidenInspector)
			{
				return -EditorGUIUtility.standardVerticalSpacing;
            }
            
            float fHeight = EditorGUI.GetPropertyHeight(property, label);
            if (!string.IsNullOrEmpty(condHAtt.HeaderString))
            {
                fHeight += HeaderHeight;
            } 
            return fHeight;
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;
			//Look for the sourcefield within the object that the property belongs to
			string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
			string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
		
			if (sourcePropertyValue != null)
			{
                var data = sourcePropertyValue.GetCurrent();
                if (data != null)
                {
                    string valueStr = data.ToString();
                    enabled = valueStr.Equals(condHAtt.ConditionalValueString);
                }
				if (condHAtt.Inverse)
					enabled = !enabled;
			}
			else
			{
                if (!(enabled = condHAtt.ConditionalValueString.Equals("null") || condHAtt.ConditionalValueString.Equals("Null")))
                    Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
                if (condHAtt.Inverse)
                    enabled = !enabled;
			}
		
			return enabled;
        }

    }
}