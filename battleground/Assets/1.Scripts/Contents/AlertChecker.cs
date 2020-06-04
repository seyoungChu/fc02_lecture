using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertChecker : MonoBehaviour
{
    [Range(0,50)] public float alertRadius;
    public int extraWaves = 1;

    public LayerMask alertMask = TagAndLayer.LayerMasking.Enemy;
    private Vector3 current;
    private bool alert;

    private void Start()
    {
        InvokeRepeating("PIngAlert", 1, 1);
    }

    private void AlertNearBy(Vector3 origin, Vector3 target, int wave = 0)
    {
        if (wave > this.extraWaves)
        {
            return;
        }
        Collider[] targetsInViewRadius = Physics.OverlapSphere(origin, alertRadius, alertMask);

        foreach(Collider obj in targetsInViewRadius)
        {
            obj.SendMessageUpwards("AlertCallback", target, SendMessageOptions.DontRequireReceiver);

            AlertNearBy(obj.transform.position, target, wave + 1);
        }

    }
    public void RootAlertNearBy(Vector3 origin)
    {
        current = origin;
        alert = true;
    }
    void PIngAlert()
    {
        if (alert)
        {
            alert = false;
            AlertNearBy(current, current);
        }
    }

}
