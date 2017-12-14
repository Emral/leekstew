using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PolyPlateau))]
public class PolyPlateauEditor : Editor
{
    //int currentPoint = 0;   // related to a feature that'd be too complicated to implement

    private int[] controlIds;


    // For disabling the regular handles
    Tool lastTool = Tool.None;

    void OnEnable()
    {
        //currentPoint = 0;

        lastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable()
    {
        Tools.current = lastTool;
    }


    public void OnSceneGUI()
    {
        PolyPlateau obj = (PolyPlateau)target;

        if (obj.checkToRefresh)
        {
            obj.checkToRefresh = false;
            obj.isDirty = true;
            EditorUtility.SetDirty(target);
            //print("A plateau is dirty...");
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        if (obj.points.Length > 0)
        {
            for (int i = 0; i < obj.points.Length; i++)
            {

                Vector3 offset = obj.transform.position;
                Vector3 origPoint = new Vector3(obj.points[i].x, 0f, obj.points[i].y);

                Handles.Label(origPoint + Vector3.up * 2f + offset, "POINT " + i.ToString());
                if ((obj.pointEditMode == PointEditType.Single && obj.editPoint == i) || obj.pointEditMode == PointEditType.All)
                {
                    Vector3 newPoint = Handles.PositionHandle(origPoint + offset, Quaternion.identity) - offset;
                    newPoint.y = 0f;

                    if (!Vector3.Equals(origPoint, newPoint))
                    {
                        obj.points[i] = new Vector2(newPoint.x, newPoint.z);
                        EditorUtility.SetDirty(target);
                        obj.isDirty = true;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
                //else
                {

                    //if (Event.current.type == EventType.MouseDown)
                    //    listenToControlId = true;
                }
            }

            //GUIUtility.GetControlID();
        }
    }
}
