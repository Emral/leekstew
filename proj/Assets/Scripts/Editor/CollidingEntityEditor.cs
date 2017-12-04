using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(CollidingEntity))]
public class CollidingEntityEditor : LeekstewEditor
{
    AnimBool m_ShowFlags;
    AnimBool m_ShowBounceToggle;
    AnimBool m_ShowBounceProps;
    AnimBool m_ShowPowerToggle;
    AnimBool m_ShowPowerProps;


    public override void OnEnable()
    {
        base.OnEnable();

        m_ShowFlags = new AnimBool(false);
        m_ShowFlags.valueChanged.AddListener(Repaint);

        m_ShowBounceProps = new AnimBool(false);
        m_ShowBounceProps.valueChanged.AddListener(Repaint);

        m_ShowBounceToggle = new AnimBool(false);
        m_ShowBounceToggle.valueChanged.AddListener(Repaint);

        m_ShowPowerProps = new AnimBool(false);
        m_ShowPowerProps.valueChanged.AddListener(Repaint);

        m_ShowPowerToggle = new AnimBool(false);
        m_ShowPowerToggle.valueChanged.AddListener(Repaint);
    }



    public override void CustomInspector()
    {
        // Collision flags
        string[] flagPropNames = { "vulnerableFlags", "harmFlags", "killFlags", "blockFlags", "pushFlags", "bounceFlags", "powerOnFlags", "powerOffFlags", "toggleFlags" };
        //ShowPropertyGroup(flagPropNames, "Collision Flags");
        TogglableFoldingPropertyGroup(flagPropNames, "Show Collision Flags", m_ShowFlags);

        // Bounce properties
        string[] bouncePropNames = { "bounceRestoresDoubleJump", "bounceStrength" };
        SerializedProperty bounceFlagsProp = serializedObject.FindProperty("bounceFlags");

        m_ShowBounceToggle.target = (bounceFlagsProp.intValue != 0);
        if (EditorGUILayout.BeginFadeGroup(m_ShowBounceToggle.faded))
        {
            TogglableFoldingPropertyGroup(bouncePropNames, "Show Bounce Properties", m_ShowBounceProps);
            EditorGUILayout.EndFadeGroup();
        }

        SerializedProperty bounceFlagsUsed = serializedObject.FindProperty("bounceFlagsUsed");
        bounceFlagsUsed.boolValue = m_ShowBounceToggle.target;


        // Power properties
        string[] powerPropNames = { "canPower", "powerCooldown", "powerTargets" };
        SerializedProperty powerOnFlagsProp = serializedObject.FindProperty("powerOnFlags");
        SerializedProperty powerOffFlagsProp = serializedObject.FindProperty("powerOffFlags");
        SerializedProperty toggleFlagsProp = serializedObject.FindProperty("toggleFlags");

        m_ShowPowerToggle.target = (powerOnFlagsProp.intValue != 0 || powerOffFlagsProp.intValue != 0 || toggleFlagsProp.intValue != 0);
        if (EditorGUILayout.BeginFadeGroup(m_ShowPowerToggle.faded))
        {
            TogglableFoldingPropertyGroup(powerPropNames, "Show Power Properties", m_ShowPowerProps);
            EditorGUILayout.EndFadeGroup();
        }

        SerializedProperty powerFlagsUsed = serializedObject.FindProperty("powerFlagsUsed");
        powerFlagsUsed.boolValue = m_ShowPowerToggle.target;


        // Default inspector
        FoldingDefaultInspector();
    }
}