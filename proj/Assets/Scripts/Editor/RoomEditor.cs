using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Room))]
public class RoomEditor : LeekstewEditor
{
    AnimBool m_changeCamera;


    public override void OnEnable()
    {
        base.OnEnable();

        m_changeCamera = new AnimBool(false);
        m_changeCamera.valueChanged.AddListener(Repaint);
    }



    public override void CustomInspector()
    {
        string[] generalProps = { "roomName", "roomId", "music", "skybox" };
        ShowPropertyGroup(generalProps, "General Properties", true);
        
        string[] cameraProps = {"newCamera"};
        SerializedProperty changeCameraProp = serializedObject.FindProperty("changeCamera");
        m_changeCamera.target = changeCameraProp.boolValue;
        TogglableFoldingPropertyGroup(cameraProps, "Change Camera?", m_changeCamera);
        changeCameraProp.boolValue = m_changeCamera.target;

        FoldingDefaultInspector();
    }

}
