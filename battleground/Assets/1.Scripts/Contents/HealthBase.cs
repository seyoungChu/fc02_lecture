using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    public class DamageInfo
    {
        public Vector3 location, direction;
        public float damage;
        public Collider bodyPart;
        public GameObject origin;

        public DamageInfo(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
        {
            this.location = location;
            this.direction = direction;
            this.damage = damage;
            this.bodyPart = bodyPart;
            this.origin = origin;
        }
    }

    [HideInInspector] public bool IsDead;
    protected Animator myAnimator;

    public virtual void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, 
        GameObject origin = null)
    {

    }

    public void HitCallBack(DamageInfo damageInfo)
    {
        this.TakeDamage(damageInfo.location, damageInfo.direction, damageInfo.damage, damageInfo.bodyPart,
            damageInfo.origin);
    }
}
