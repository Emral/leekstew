using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReorderableListAttribute))]
public class ListDrawer : PropertyDrawer
{
    private const float ITEMSIZE = 15f;


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return base.GetPropertyHeight(property, label) + ITEMSIZE * (property.CountInProperty());
        }
        else
        {
            return base.GetPropertyHeight(property, label);
        }
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //ReorderableListAttribute listSettings = (ReorderableListAttribute)attribute;
        //SerializedProperty targetArray = GetBaseProperty<Array>(property);
        /*
        string path = property.propertyPath;
        int arrayInd = path.LastIndexOf(".Array");
        bool bIsArray = arrayInd >= 0;

        if (bIsArray)
        {
            SerializedObject so = property.serializedObject;
            string arrayPath = path.Substring(0, arrayInd);
            SerializedProperty arrayProp = so.FindProperty(arrayPath);

            int indStart = path.IndexOf("[") + 1;
            int indEnd = path.IndexOf("]");
            string indString = path.Substring(indStart, indEnd - indStart);
            int myIndex = int.Parse(indString);

            if (myIndex == 0)
            {
                UnityEditorInternal.ReorderableList list = new UnityEditorInternal.ReorderableList(so, arrayProp, true, true, true, true);
                list.DoList(position);
            }
        }
        */
        //string propName = listSettings.listName;
        //if (string.IsNullOrEmpty(listName))
        //    propName = property.name;

        //*

        string path = property.propertyPath;
        int arrayInd = path.LastIndexOf(".Array");

        string arrayPath = path.Substring(0, arrayInd);
        SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);

        int indStart = path.IndexOf("[") + 1;
        int indEnd = path.IndexOf("]");
        string indString = path.Substring(indStart, indEnd - indStart);
        int myIndex = int.Parse(indString);

        bool moveUp = false;
        bool moveDown = false;

        float baseHeight = base.GetPropertyHeight(property, label);

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width-50, position.height), property, true);
        if (myIndex > 0)
            moveUp = GUI.Button(new Rect(position.x + position.width - 40, position.y, 20, baseHeight), "U");
        if (myIndex < arrayProp.arraySize-1)
            moveDown = GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, baseHeight), "D");

        if (moveUp)
            arrayProp.MoveArrayElement(myIndex, myIndex - 1);
        if (moveDown)
            arrayProp.MoveArrayElement(myIndex, myIndex + 1);
        //EditorGUI
        EditorGUI.EndProperty();
        //*/
    }

    static T GetBaseProperty<T>(SerializedProperty prop)
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
}