using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StateController))]
public class FieldOfViewEditor : Editor
{
    Vector3 DirFromAngle(Transform transform, float angleInDegrees, bool angleIsGlobal)
    { 
        if(!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, 
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));        
    }
    private void OnSceneGUI()
    {
        StateController fov = target as StateController;
        if(fov == null || fov.gameObject == null)
        {
            return;
        }

        Handles.color = Color.white;
        //Perception Area (circle)
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.perceptionRadius);
        //near
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360,
            fov.perceptionRadius * 0.5f);

        Vector3 viewAngleA = DirFromAngle(fov.transform, -fov.viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(fov.transform, fov.viewAngle / 2, false);

        Handles.DrawWireArc(fov.transform.position, Vector3.up, viewAngleA, fov.viewAngle, fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.yellow;
        if(fov.targetInSight && fov.personalTarget != Vector3.zero)
        {
            Handles.DrawLine(fov.enemyAnimation.gunMuzzle.position, fov.personalTarget);
        }           

    }
}
