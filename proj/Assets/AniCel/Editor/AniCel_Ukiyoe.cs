using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AniCel_Ukiyoe : AniCel_Anime
{

    protected override void MainMaps(MaterialEditor editor, MaterialProperty[] properties)
    {
        Material target = editor.target as Material;
        UpdateShadingType(target);
        UpdateRenderMode(target);
        ShadingTypeAndColor(target, editor, properties, false);

        EditorGUI.BeginChangeCheck();
        MaterialProperty bumpmap = Texture("_BumpMap", "Normal Map", true, editor, properties);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(target, "ANICEL_NORMALMAP", bumpmap.textureValue);
        }
    }

    protected virtual void Stroke(MaterialEditor editor, MaterialProperty[] properties)
    {
        Slider("_Brightness", editor, properties);
        Slider("_Saturation", editor, properties);
        Texture("_StrokeMask", "Stroke Mask (RGB)", true, editor, properties, FindProperty("_StrokeStrength", properties));
    }

    protected override void Shadows(MaterialEditor editor, MaterialProperty[] properties)
    {
        GUILayout.Label("Shading", EditorStyles.boldLabel);
        if (!KeywordToggle("ANICEL_UKIYOE_UNLIT", "Unlit", editor))
        {
            if (KeywordToggle("ANICEL_UKIYOE_SHADING", "Enable Shading", editor))
            {
                Slider("_ShadowSharpness", editor, properties);
                Slider("_ShadowDepth", editor, properties);
                Slider("_ShadowOffset", editor, properties);
            }
        }

        Slider("_OutlineWidth", editor, properties);
        Slider("_OutlineBrightness", editor, properties);
        Slider("_OutlineSpace", editor, properties);
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        MainMaps(editor, properties);
        Stroke(editor, properties);
        Shadows(editor, properties);
        Advanced(editor, properties);
    }
}
