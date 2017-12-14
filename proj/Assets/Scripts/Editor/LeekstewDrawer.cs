using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

public class LeekstewDrawer : PropertyDrawer
{
    SerializedProperty currentProperty;


    public virtual void OnEnable()
    {
    }


    public void ShowPropertyGroup(string[] properties, string groupName = "", bool enclose = false)
    {
        if (enclose)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
        }

        if (groupName != "")
        {
            EditorGUILayout.LabelField(groupName);
        }

        if (enclose)
        {
            EditorGUI.indentLevel++;
        }

        foreach (string propName in properties)
        {
            SerializedProperty prop = currentProperty.FindPropertyRelative(propName);
            EditorGUILayout.PropertyField(prop, true);
        }

        if (enclose)
        {
            //EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        if (groupName != "" && enclose)
            EditorGUILayout.Space();
    }

    public void FoldingPropertyGroup(string[] properties, string groupName, AnimBool toggleBool, bool enclose = true)
    {
        if (EditorGUILayout.BeginFadeGroup(toggleBool.faded))
        {
            ShowPropertyGroup(properties, groupName, enclose);
            EditorGUILayout.EndFadeGroup();
        }
    }

    public void TogglableFoldingPropertyGroup(string[] properties, string groupName, AnimBool toggleBool, string togglePropName = "", bool enclose = true)
    {
        if (enclose)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
        }

        toggleBool.target = EditorGUILayout.ToggleLeft(groupName, toggleBool.target);

        if (enclose)
            EditorGUI.indentLevel++;

        FoldingPropertyGroup(properties, "", toggleBool, false);

        if (enclose)
        {
            //EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();

        if (togglePropName != "")
        {
            SerializedProperty toggleProp = currentProperty.FindPropertyRelative(togglePropName);
            if (toggleProp != null)
                toggleProp.boolValue = toggleBool.target;
        }
    }


    /*
    public static T GetActualObjectForSerializedProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
    {
        var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
        if (obj == null) { return null; }

        T actualObject = null;
        if (obj.GetType().IsArray)
        {
            var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
            actualObject = ((T[])obj)[index];
        }
        else
        {
            actualObject = obj as T;
        }
        return actualObject;
    }
    */

    public static T GetBaseProperty<T>(SerializedProperty prop)
    {
        // Separate the steps it takes to get to this property
        string[] separatedPaths = prop.propertyPath.Split('.');

        // Go down to the root of this serialized property
        System.Object reflectionTarget = prop.serializedObject.targetObject as object;
        // Walk down the path to get the target object
        foreach (var path in separatedPaths)
        {
            FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
            reflectionTarget = fieldInfo.GetValue(reflectionTarget);
        }
        return (T)reflectionTarget;
    }



    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        currentProperty = property;
        CustomDrawer(position, property, label);
    }

    public virtual void CustomDrawer(Rect position, SerializedProperty property, GUIContent label)
    {
    }
}