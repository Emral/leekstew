using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AniCel_Anime : ShaderGUI
{
    protected enum ShadingType
    {
        Uniform, Lines, Dots, Mask
    }

    protected enum RenderingMode
    {
        Opaque, Cutout, Fade, Transparent
    }


    protected ShadingType shading = ShadingType.Uniform;
    protected RenderingMode renderMode = RenderingMode.Opaque;

    protected void UpdateShadingType(Material target)
    {
        if (target.IsKeywordEnabled("ANICEL_LINES") || target.IsKeywordEnabled("ANICEL_LINES_DYNAMIC"))
        {
            shading = ShadingType.Lines;
        }
        else if (target.IsKeywordEnabled("ANICEL_DOTS") || target.IsKeywordEnabled("ANICEL_DOTS_DYNAMIC"))
        {
            shading = ShadingType.Dots;
        }
        else if (target.IsKeywordEnabled("ANICEL_MASK"))
        {
            shading = ShadingType.Mask;
        }
        else
        {
            shading = ShadingType.Uniform;
        }
    }

    protected void UpdateRenderMode(Material target)
    {
        if (target.IsKeywordEnabled("ANICEL_RENDER_CUTOFF"))
        {
            renderMode = RenderingMode.Cutout;
        }
        else if (target.IsKeywordEnabled("ANICEL_RENDER_FADE"))
        {
            renderMode = RenderingMode.Fade;
        }
        else if(target.IsKeywordEnabled("ANICEL_RENDER_TRANSPARENT"))
        {
            renderMode = RenderingMode.Transparent;
        }
        else
        {
            renderMode = RenderingMode.Opaque;
        }
    }

    protected GUIContent Label(MaterialProperty p, string alttext)
    {
        return new GUIContent(p.displayName, alttext);
    }

    protected void Slider(string property, MaterialEditor editor, MaterialProperty[] properties)
    {
        MaterialProperty slider = FindProperty(property, properties);
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, Label(slider, null));
        EditorGUI.indentLevel -= 2;
    }

    protected MaterialProperty Texture(string property, string alttext, bool conditionalScale, MaterialEditor editor, MaterialProperty[] properties, MaterialProperty extra1 = null, MaterialProperty extra2 = null)
    {
        MaterialProperty mainTex = FindProperty(property, properties);
        editor.TexturePropertySingleLine(Label(mainTex, alttext), mainTex, extra1, extra2);
        if (!conditionalScale || mainTex.textureValue)
        {
            editor.TextureScaleOffsetProperty(mainTex);
        }
        return mainTex;
    }

    protected bool SetKeyword(Material target, string keyword, bool value)
    {
        bool b = target.IsKeywordEnabled(keyword);
        if (value && !b)
        {
            target.EnableKeyword(keyword);
        }
        else if (!value && b)
        {
            target.DisableKeyword(keyword);
        }
        return b;
    }

    protected bool KeywordToggle(string keyword, string label, MaterialEditor editor)
    {
        Material target = editor.target as Material;
        return SetKeyword(target, keyword, EditorGUILayout.Toggle(label, target.IsKeywordEnabled(keyword)));
    }

    protected virtual void ShadingTypeAndColor(Material target, MaterialEditor editor, MaterialProperty[] properties, bool showGreyscale = true)
    {
        EditorGUI.BeginChangeCheck();
        editor.RegisterPropertyChangeUndo("Rendering Mode");
        renderMode = (RenderingMode)EditorGUILayout.EnumPopup(new GUIContent("Rendering Mode"), renderMode);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(target, "ANICEL_RENDER_CUTOFF", renderMode == RenderingMode.Cutout);
            SetKeyword(target, "ANICEL_RENDER_FADE", renderMode == RenderingMode.Fade);
            SetKeyword(target, "ANICEL_RENDER_TRANSPARENT", renderMode == RenderingMode.Transparent);

            RenderQueue queue;
            string renderType;
            BlendMode srcBlend, dstBlend;
            bool zWrite;

            switch (renderMode)
            {
                case RenderingMode.Cutout:
                    queue = RenderQueue.AlphaTest;
                    renderType = "TransparentCutout";
                    srcBlend = BlendMode.One;
                    dstBlend = BlendMode.Zero;
                    zWrite = true;
                    break;
                case RenderingMode.Fade:
                    queue = RenderQueue.Transparent;
                    renderType = "Transparent";
                    srcBlend = BlendMode.SrcAlpha;
                    dstBlend = BlendMode.OneMinusSrcAlpha;
                    zWrite = false;
                    break;
                case RenderingMode.Transparent:
                    queue = RenderQueue.Transparent;
                    renderType = "Transparent";
                    srcBlend = BlendMode.One;
                    dstBlend = BlendMode.OneMinusSrcAlpha;
                    zWrite = false;
                    break;
                case RenderingMode.Opaque:
                default:
                    queue = RenderQueue.Geometry;
                    renderType = "";
                    srcBlend = BlendMode.One;
                    dstBlend = BlendMode.Zero;
                    zWrite = true;
                    break;
            }

            foreach (Material m in editor.targets)
            {
                m.renderQueue = (int)queue;
                m.SetOverrideTag("RenderType", renderType);
                m.SetInt("_SrcBlend", (int)srcBlend);
                m.SetInt("_DstBlend", (int)dstBlend);
                m.SetInt("_ZWrite", zWrite ? 1 : 0);
            }
        }

        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        MaterialProperty mainTex = FindProperty("_MainTex", properties);
        editor.TexturePropertySingleLine(Label(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Color", properties));
        if (renderMode == RenderingMode.Cutout)
        {
            Slider("_Cutoff", editor, properties);
        }
        EditorGUI.indentLevel += 2;
        target.SetInt("_Cull", EditorGUILayout.Toggle("Two-Sided", target.GetInt("_Cull") == 0) ? 0 : 2);
        if (showGreyscale)
        {
            KeywordToggle("ANICEL_MANGA", "Greyscale", editor);
        }
        EditorGUI.indentLevel -= 2;
        editor.TextureScaleOffsetProperty(mainTex);
    }

    protected virtual void Specular(MaterialEditor editor, MaterialProperty[] properties)
    {
        if (KeywordToggle("ANICEL_SPECULAR", "Enable Specularity", editor))
        {
            Texture("_SpecGlossMap", "Specular (RGB) and Smoothness(A)", true, editor, properties, FindProperty("_SpecColor", properties));

            Slider("_Glossiness", editor, properties);
            Slider("_SpecLevels", editor, properties);
            Slider("_SpecSharpness", editor, properties);

            switch (shading)
            {
                case ShadingType.Lines:
                    Slider("_SpecLineStroke", editor, properties);
                    if (!((Material)editor.target).IsKeywordEnabled("ANICEL_LINES_DYNAMIC"))
                    {
                        Slider("_SpecLineDensity", editor, properties);
                    }
                    break;
                case ShadingType.Dots:
                    Slider("_SpecDotRadius", editor, properties);
                    if (!((Material)editor.target).IsKeywordEnabled("ANICEL_DOTS_DYNAMIC"))
                    {
                        Slider("_SpecDotDensity", editor, properties);
                    }
                    break;
                case ShadingType.Mask:
                    Texture("_SpecBrushMask", "Specular Brush Mask (R)", true, editor, properties);
                    break;
            }
        }
    }

    protected virtual void ShadingStyle(Material target, MaterialEditor editor, MaterialProperty[] properties)
    {
        EditorGUI.BeginChangeCheck();
        editor.RegisterPropertyChangeUndo("Shading Style");
        ShadingType lastType = shading;
        shading = (ShadingType)EditorGUILayout.EnumPopup(new GUIContent("Style"), shading);
        if(lastType != shading)
        {
            if(lastType == ShadingType.Dots && shading == ShadingType.Lines)
            {
                SetKeyword(target, "ANICEL_LINES_DYNAMIC", target.IsKeywordEnabled("ANICEL_DOTS_DYNAMIC"));
            }
            else if (lastType == ShadingType.Lines && shading == ShadingType.Dots)
            {
                SetKeyword(target, "ANICEL_DOTS_DYNAMIC", target.IsKeywordEnabled("ANICEL_LINES_DYNAMIC"));
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(target, "ANICEL_MASK", shading == ShadingType.Mask);
        }
        switch (shading)
        {
            case ShadingType.Lines:
                KeywordToggle("ANICEL_LINES_DYNAMIC", "Dynamic Density", editor);
                SetKeyword(target, "ANICEL_LINES", !target.IsKeywordEnabled("ANICEL_LINES_DYNAMIC"));
                SetKeyword(target, "ANICEL_DOTS", false);
                SetKeyword(target, "ANICEL_DOTS_DYNAMIC", false);
                break;
            case ShadingType.Dots:
                KeywordToggle("ANICEL_DOTS_DYNAMIC", "Dynamic Density", editor);
                SetKeyword(target, "ANICEL_DOTS", !target.IsKeywordEnabled("ANICEL_DOTS_DYNAMIC"));
                SetKeyword(target, "ANICEL_LINES", false);
                SetKeyword(target, "ANICEL_LINES_DYNAMIC", false);
                break;
            default:
                SetKeyword(target, "ANICEL_LINES", false);
                SetKeyword(target, "ANICEL_LINES_DYNAMIC", false);
                SetKeyword(target, "ANICEL_DOTS", false);
                SetKeyword(target, "ANICEL_DOTS_DYNAMIC", false);
                break;
        }
    }

    protected virtual void MainMaps(MaterialEditor editor, MaterialProperty[] properties)
    {
        Material target = editor.target as Material;
        UpdateShadingType(target);
        UpdateRenderMode(target);
        ShadingTypeAndColor(target, editor, properties);

        EditorGUI.BeginChangeCheck();
        MaterialProperty bumpmap = Texture("_BumpMap", "Normal Map", true, editor, properties);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(target, "ANICEL_NORMALMAP", bumpmap.textureValue);
        }
        
        GUILayout.Label("Shading Style", EditorStyles.boldLabel);
        ShadingStyle(target, editor, properties);

        GUILayout.Label("Specularity", EditorStyles.boldLabel);
        Specular(editor, properties);
    }

    protected virtual void Shadows(MaterialEditor editor, MaterialProperty[] properties)
    {
        GUILayout.Label("Shading", EditorStyles.boldLabel);
        
        if (KeywordToggle("ANICEL_FRESNEL", "Enable Fresnel", editor))
        {
            MaterialProperty h = FindProperty("_FresColor", properties);
            EditorGUI.indentLevel += 2;
            editor.ColorProperty(h, h.displayName);
            EditorGUI.indentLevel -= 2;
            Slider("_FresPower", editor, properties);
        }

        MaterialProperty hue = FindProperty("_ShadowHue", properties);
        EditorGUI.indentLevel += 2;
        editor.ColorProperty(hue, hue.displayName);
        EditorGUI.indentLevel -= 2;
        Slider("_ShadowSaturation", editor, properties);
        Slider("_ShadowSharpness", editor, properties);
        Slider("_ShadowDepth", editor, properties);
        Slider("_ShadowOffset", editor, properties);

        switch (shading)
        {
            case ShadingType.Lines:
                Slider("_ShadowLineStroke", editor, properties);
                if (!((Material)editor.target).IsKeywordEnabled("ANICEL_LINES_DYNAMIC"))
                {
                    Slider("_ShadowLineDensity", editor, properties);
                }
                break;
            case ShadingType.Dots:
                Slider("_ShadowDotRadius", editor, properties);
                if (!((Material)editor.target).IsKeywordEnabled("ANICEL_DOTS_DYNAMIC"))
                {
                    Slider("_ShadowDotDensity", editor, properties);
                }
                break;
            case ShadingType.Mask:
                Texture("_ShadowBrushMask", "Specular Brush Mask (R)", true, editor, properties);
                break;
        }
    }

    protected void Advanced(MaterialEditor editor, MaterialProperty[] properties)
    {
        GUILayout.Label("Advanced Options", EditorStyles.boldLabel);
        editor.EnableInstancingField();
        editor.DoubleSidedGIField();
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        MainMaps(editor, properties);
        Shadows(editor, properties);
        Advanced(editor, properties);
    }
}
