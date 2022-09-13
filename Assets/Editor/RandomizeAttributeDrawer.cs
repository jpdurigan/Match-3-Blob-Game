using UnityEngine;
using UnityEditor;

// [CustomPropertyDrawer(typeof(RandomizeAttribute))]
public class RandomizeAttributeDrawer : PropertyDrawer
{
    const float FIELD_HEIGHT_STANDART = 16f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return FIELD_HEIGHT_STANDART * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect labelPosition = new Rect(position.x, position.y, position.width, FIELD_HEIGHT_STANDART);
            Rect buttonPosition = new Rect(position.x, position.y + labelPosition.height, position.width, FIELD_HEIGHT_STANDART);
            EditorGUI.LabelField(labelPosition, "teste");
            EditorGUI.PropertyField(labelPosition, property);
            if (GUI.Button(buttonPosition, "Randomize"))
            {
                // RandomizeAttribute randomizeAttribute = (RandomizeAttribute)attribute;
                // property.floatValue = Random.Range(randomizeAttribute.minValue, randomizeAttribute.maxValue);
            }
            EditorGUI.EndProperty();
        }
        else{
            EditorGUI.LabelField(position, "Randomize Atrribute should only be used in floats!");
        }
    }
}
