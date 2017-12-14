using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

public class LeekstewEditor : Editor
{
    static GUIStyle enclosedStyle;
    AnimBool m_ShowDefaultInspector;

    public virtual void OnEnable()
    {
        //if (enclosedStyle == null)
        {
            //enclosedStyle = new GUIStyle(GUI.skin.box);
            //enclosedStyle.color = Color.gray;
        }

        m_ShowDefaultInspector = new AnimBool(false);
        m_ShowDefaultInspector.valueChanged.AddListener(Repaint);
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
            SerializedProperty prop = serializedObject.FindProperty(propName);
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
        //CollidingEntity targetCollidingEntity = (CollidingEntity)target;

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
            SerializedProperty toggleProp = serializedObject.FindProperty(togglePropName);
            if (toggleProp != null)
                toggleProp.boolValue = toggleBool.target;
        }
    }

    public void FoldingDefaultInspector()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        m_ShowDefaultInspector.target = EditorGUILayout.ToggleLeft("Show Default Inspector", m_ShowDefaultInspector.target);

        EditorGUI.indentLevel++;
        if (EditorGUILayout.BeginFadeGroup(m_ShowDefaultInspector.faded))
        {
            DrawDefaultInspector();
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    public virtual void CustomInspector()
    {
        // Default inspector
        DrawDefaultInspector();
    }

    public override void OnInspectorGUI()
    {
        CustomInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
