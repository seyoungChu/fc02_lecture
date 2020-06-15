using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
public class EnemyAnimation : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public float currentAimingAngleGap;
    [HideInInspector] public Transform gunMuzzle;
    [HideInInspector] public float angularSpeed;

    private StateController controller;
    private NavMeshAgent nav;
    private bool pendingAim; //조준을 기다리는 시간.
    private Transform hips, spine; // bone trans//
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Quaternion lastRotation;
    private float timeCountAim, timeCountGuard;
    private readonly float turnSpeed = 25f; //strafing turn speed.

    private void Awake()
    {
        //setup
        controller = GetComponent<StateController>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        nav.updateRotation = false;

        hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);

        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;

        anim.SetTrigger(AnimatorKey.ChangeWeapon);
        anim.SetInteger(AnimatorKey.Weapon, (int)Enum.Parse(typeof(WeaponType), 
            controller.classStats.WeaponType));

        foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
        {
            gunMuzzle = child.Find("muzzle");
            if (gunMuzzle != null)
            {
                break;
            }
        }
        foreach(Rigidbody member in GetComponentsInChildren<Rigidbody>())
        {
            member.isKinematic = true;
        }
    }

    void Setup(float speed, float angle, Vector3 strafeDirection)
    {
        angle *= Mathf.Deg2Rad;
        angularSpeed = angle / controller.generalStats.angleResponseTime;

        anim.SetFloat(AnimatorKey.Speed, speed, controller.generalStats.speedDampTime, Time.deltaTime);
        anim.SetFloat(AnimatorKey.AngularSpeed, angularSpeed, controller.generalStats.angularSpeedDampTime,
            Time.deltaTime);

        anim.SetFloat(AnimatorKey.Horizontal, strafeDirection.x, controller.generalStats.speedDampTime,
            Time.deltaTime);
        anim.SetFloat(AnimatorKey.Vertical, strafeDirection.z, controller.generalStats.speedDampTime,
            Time.deltaTime);
    }
    
    void NavAnimSetup()
    {
        float speed;
        float angle;
        speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;
        if(controller.focusSight)
        {
            Vector3 dest = (controller.personalTarget - transform.position);
            dest.y = 0.0f;
            angle = Vector3.SignedAngle(transform.forward, dest, transform.up);
            if(controller.Strafing)
            {
                dest = dest.normalized;
                Quaternion targetStrafeRotation = Quaternion.LookRotation(dest);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    targetStrafeRotation, turnSpeed * Time.deltaTime);
            }

        }
        else
        {
            if(nav.desiredVelocity == Vector3.zero)
            {
                angle = 0.0f;
            }
            else
            {
                angle = Vector3.SignedAngle(transform.forward, nav.desiredVelocity, transform.up);
            }
        }
        //플레이어를 향하려 할때 깜빡 꺼리지 않도록 각도 데드존을 적용.
        if(!controller.Strafing && Mathf.Abs(angle) < controller.generalStats.angleDeadZone)
        {
            transform.LookAt(transform.position + nav.desiredVelocity);
            angle = 0f;
            if(pendingAim && controller.focusSight)
            {
                controller.Aiming = true;
                pendingAim = false;
            }
        }
        
        //Strafe direction
        Vector3 direction = nav.desiredVelocity;
        direction.y = 0.0f;
        direction = direction.normalized;
        direction = Quaternion.Inverse(transform.rotation) * direction;
        Setup(speed, angle, direction);
    }
    private void Update()
    {
        NavAnimSetup();
    }
    private void OnAnimatorMove()
    {
        if(Time.timeScale > 0 && Time.deltaTime > 0)
        {
            nav.velocity = anim.deltaPosition / Time.deltaTime;
            if(!controller.Strafing)
            {
                transform.rotation = anim.rootRotation;
            }
        }
    }
    private void LateUpdate()
    {
        if(controller.Aiming)
        {
            Vector3 direction = controller.personalTarget - spine.position;
            if(direction.magnitude < 0.01f || direction.magnitude > 1000000.0f)
            {
                return;
            }
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipsRotation);
            targetRotation *= Quaternion.Euler(initialSpineRotation);

            targetRotation *= Quaternion.Euler(VectorHelper.ToVector(controller.classStats.AimOffset));
            Quaternion frameRotation = Quaternion.Slerp(lastRotation, targetRotation, timeCountAim);
            //엉덩이를 기준으로 척추 회전이 60도 이하인경우는 계속 조준이 가능.
            if(Quaternion.Angle(frameRotation, hips.rotation) <= 60.0f)
            {
                spine.rotation = frameRotation;
                timeCountAim += Time.deltaTime;
            }
            else
            {
                if(timeCountAim == 0 && Quaternion.Angle(frameRotation, hips.rotation) > 70.0f)
                {
                    StartCoroutine(controller.UnstuckAim(2f));
                }
                spine.rotation = lastRotation;
                timeCountAim = 0;
            }

            lastRotation = spine.rotation;
            Vector3 target = controller.personalTarget - gunMuzzle.position;
            Vector3 forward = gunMuzzle.forward;
            currentAimingAngleGap = Vector3.Angle(target, forward);

            timeCountGuard = 0;
        }
        else
        {
            lastRotation = spine.rotation;
            spine.rotation *= Quaternion.Slerp(Quaternion.Euler(VectorHelper.ToVector(
                controller.classStats.AimOffset)), Quaternion.identity, timeCountGuard);
            timeCountGuard += Time.deltaTime;
        }
    }

    public void ActivatePendingAim()
    {
        pendingAim = true;
    }
    public void AbortPendingAim()
    {
        pendingAim = false;
        controller.Aiming = false;
    }


}
