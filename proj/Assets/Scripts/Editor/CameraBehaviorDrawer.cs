using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(CameraBehavior))]
public class CameraBehaviorDrawer : LeekstewDrawer
{
    Dictionary<string, CameraProperties> propEnumPairs;
    Dictionary<string, string[]> propRows;

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void CustomDrawer(Rect position, SerializedProperty property, GUIContent label)
    {
        propRows = new Dictionary<string, string[]>();
        propRows["yaw"] = new string[] { "yaw", "pitch" };
        propRows["pitch"] = new string[] { "yaw", "pitch" };
        propRows["panX"] = new string[] { "panX", "panY", "zoom" };
        propRows["panY"] = new string[] { "panX", "panY", "zoom" };
        propRows["zoom"] = new string[] { "panX", "panY", "zoom" };

        propEnumPairs = new Dictionary<string, CameraProperties>();
        propEnumPairs["yaw"] = CameraProperties.Yaw;
        propEnumPairs["pitch"] = CameraProperties.Pitch;
        propEnumPairs["panX"] = CameraProperties.XPan;
        propEnumPairs["panY"] = CameraProperties.YPan;
        propEnumPairs["zoom"] = CameraProperties.Zoom;
        propEnumPairs["position"] = CameraProperties.Position;
        propEnumPairs["target"] = CameraProperties.Target;
        propEnumPairs["regionMin"] = CameraProperties.Region;
        propEnumPairs["regionMax"] = CameraProperties.Region;

        CameraBehavior targetBehavior = GetBaseProperty<CameraBehavior>(property);

        ResetCurrentLine();

        position = EditorGUI.PrefixLabel(GetCurrentLine(), GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUI.BeginProperty(position, label, property);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;
        var labelW = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth *= 0.75f;

        EditorGUI.PropertyField(GetNextLine(), property.FindPropertyRelative("debugName"));

        Rect newPos = GetNextLine();
        newPos.width *= 0.5f;
        EditorGUIUtility.labelWidth = 0.75f * labelW;
        EditorGUI.PropertyField(newPos, property.FindPropertyRelative("easeTime"));
        newPos.x += newPos.width;
        EditorGUIUtility.labelWidth = 0.5f * labelW;
        EditorGUI.PropertyField(newPos, property.FindPropertyRelative("priority"));

        HalfLine();

        EditorGUIUtility.labelWidth = 1.25f * labelW;
        targetBehavior.changedProperties = (CameraProperties)EditorGUI.EnumMaskField(GetNextLine(), "Affected Properties", targetBehavior.changedProperties);

        HalfLine();

        List<string> keysToSkip = new List<string>();
        foreach (string propKey in propEnumPairs.Keys)
        {
            EditorGUIUtility.labelWidth = 0.75f * labelW;
            if (!keysToSkip.Contains(propKey) && FlagsHelper.IsSet(targetBehavior.changedProperties, propEnumPairs[propKey]))
            {
                if (propRows.ContainsKey(propKey))
                {
                    GetNextLine();

                    string[] row = propRows[propKey];
                    bool[] includeFlags = new bool[row.Length];
                    for (int i = 0; i < row.Length; i++)
                    {
                        string rowPropName = row[i];

                        includeFlags[i] = FlagsHelper.IsSet(targetBehavior.changedProperties, propEnumPairs[rowPropName]);
                        keysToSkip.Add(rowPropName);
                    }
                    PropertyRow(row, includeFlags);
                }
                else
                    EditorGUI.PropertyField(GetNextLine(), property.FindPropertyRelative(propKey));
            }
        }

        EditorGUI.indentLevel = indent;
        EditorGUIUtility.labelWidth = labelW;
        EditorGUI.EndProperty();
    }

}
