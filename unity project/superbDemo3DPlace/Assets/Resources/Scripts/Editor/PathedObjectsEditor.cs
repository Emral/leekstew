using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathedObjects))]
public class PathedObjectsEditor : Editor
{
    public void OnSceneGUI()
    {
        PathedObjects obj = (PathedObjects)target;

        if (obj.transform.childCount != obj.cachedCount)
            obj.isDirty = true;

        if (obj.points.Length > 0)
        {
            for (int i = 1; i < obj.points.Length; i++)
            {
                Vector3 offset = obj.relative ? obj.transform.position : Vector3.zero;
                Vector3 newPoint = Handles.PositionHandle(offset+obj.points[i], Quaternion.identity) - offset;

                if (!Vector3.Equals(obj.points[i], newPoint))
                {
                    obj.points[i] = newPoint;
                    obj.isDirty = true;
                }
            }
        }
    }
}