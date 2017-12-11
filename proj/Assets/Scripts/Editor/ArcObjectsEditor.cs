using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArcObjects))]
public class ArcObjectsEditor : Editor
{
    public void OnSceneGUI()
    {
        ArcObjects obj = (ArcObjects)target;

        if (obj.transform.childCount != obj.cachedCount && obj.updateAutomatically)
            obj.isDirty = true;

        if (obj.gap > 0)
        {
            Vector3 offset = obj.transform.position;

            Vector3 newEndPoint = Handles.PositionHandle(offset + obj.endPoint, Quaternion.identity) - offset;
            Vector3 newControlPoint = Handles.PositionHandle(offset + obj.controlPoint, Quaternion.identity) - offset;

            if (!Vector3.Equals(obj.endPoint, newEndPoint))
            {
                obj.endPoint = newEndPoint;
                if (obj.updateAutomatically)
                    obj.isDirty = true;
            }
            if (!Vector3.Equals(obj.controlPoint, newControlPoint))
            {
                obj.controlPoint = newControlPoint;
                if (obj.updateAutomatically)
                    obj.isDirty = true;
            }
        }
    }
}