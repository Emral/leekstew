using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntRange))]
public class IntRangeInspector : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty maxProp = property.FindPropertyRelative("_maxValue");
        SerializedProperty minProp = property.FindPropertyRelative("_minValue");
        SerializedProperty tMaxProp = property.FindPropertyRelative("_totalMaxValue");
        SerializedProperty tMinProp = property.FindPropertyRelative("_totalMinValue");

        float minVal = minProp.intValue + 0f;
        float maxVal = maxProp.intValue + 0f;
        float tMinVal = tMinProp.intValue + 0f;
        float tMaxVal = tMaxProp.intValue + 0f;

        Rect rFull = new Rect(position.xMin, position.yMin + EditorGUIUtility.singleLineHeight*0.5f, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginProperty(rFull, label, property);

        EditorGUI.MinMaxSlider(rFull, label, ref minVal, ref maxVal, tMinProp.intValue + 0f, tMaxProp.intValue + 0f);

        minProp.intValue = Mathf.RoundToInt(minVal);
        maxProp.intValue = Mathf.RoundToInt(maxVal);

        float indentWidth = EditorGUIUtility.labelWidth;
        float zeroedMin = minProp.intValue + 0f - tMinVal;
        float zeroedMax = maxProp.intValue + 0f - tMinVal;
        float zeroedTMax = tMaxVal - tMinVal;

        Rect rNumbers = new Rect(rFull.left + indentWidth + (zeroedMin/zeroedTMax)*(rFull.width-indentWidth), position.yMin, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rNumbers, minProp.intValue.ToString() + "-" + maxProp.intValue.ToString());
        
        EditorGUI.EndProperty();
    }
}