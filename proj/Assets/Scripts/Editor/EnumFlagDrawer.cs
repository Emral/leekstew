using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : LeekstewDrawer
{
    public override void CustomDrawer(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumFlagAttribute flagSettings = (EnumFlagAttribute)attribute;
        Enum targetEnum = GetBaseProperty<Enum>(property);

        string propName = flagSettings.enumName;
        if (string.IsNullOrEmpty(propName))
            propName = property.name;

        EditorGUI.BeginProperty(position, label, property);
        Enum enumNew = EditorGUI.EnumMaskField(position, propName, targetEnum);
        property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
        EditorGUI.EndProperty();
    }
}