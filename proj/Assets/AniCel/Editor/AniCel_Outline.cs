using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AniCel_Outline : AniCel_Anime
{
    protected override void ShadingStyle(Material target, MaterialEditor editor, MaterialProperty[] properties)
    {
        base.ShadingStyle(target, editor, properties);

        MaterialProperty hue = FindProperty("_OutlineColor", properties);
        EditorGUI.indentLevel += 2;
        editor.ColorProperty(hue, hue.displayName);
        EditorGUI.indentLevel -= 2;
        Slider("_OutlineWidth", editor, properties);
        Slider("_OutlineSpace", editor, properties);
    }
}
