using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/*
[CustomPropertyDrawer(typeof(SoundArrayDictEntry))]
public class ClipArrayDictDrawer : LeekstewDrawer
{
    int extra = 0;

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + extra*EditorGUIUtility.singleLineHeight;
    }


    public override void CustomDrawer(Rect position, SerializedProperty property, GUIContent label)
    {
        //SoundArrayDictEntry targetEntry = GetBaseProperty<SoundArrayDictEntry>(property);
        ResetCurrentLine();
        extra = 0;

        EditorGUI.BeginProperty(position, label, property);

        Rect box = GetCurrentLine();
        box.width *= 0.4f;

        var labelW = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth *= 0.45f;

        EditorGUI.PropertyField(box, property.FindPropertyRelative("key"));

        SerializedProperty arrayProp = property.FindPropertyRelative("clips");

        Rect boxB = GetCurrentLine();
        boxB.xMin += box.width;
        EditorGUI.PropertyField(boxB, arrayProp.FindPropertyRelative("Array.size"), true);

        extra = arrayProp.arraySize;
        for (int i = 0; i < extra; i+=2)
        {
            if (i < extra)
            {
                Rect next = GetNextLine();
                next.width *= 0.5f;
                EditorGUI.PropertyField(next, arrayProp.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
                if (i+1 < extra)
                {
                    next.x += next.width;
                    EditorGUI.PropertyField(next, arrayProp.GetArrayElementAtIndex(i+1), new GUIContent((i+1).ToString()));
                }
            }
        }

        EditorGUIUtility.labelWidth = labelW;
        EditorGUI.EndProperty();
    }
    
}
*/