using FC;
using NPOI.SS.Formula.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 총 4단계에 걸쳐 사격:
/// 1. 조준 중이고 조준 유효 각도 안에 타겟이 있거나 가깝다면
/// 2. 발사 간격 딜레이가 충분히 되었다면 애니메이션을 재생
/// 3. 총돌 검출을 하는데 약간의 사격시 충격파도 더해주게 됩니다.
/// 4. 총구 이펙트 및 총알 이펙트를 생성해 줍니다.
/// </summary>
/// 
[CreateAssetMenu(menuName = "PluggableAI/Actions/Attack")]
public class AttackAction : Action
{
    private readonly float startShootDelay = 0.2f;
    private readonly float aimAngleGap = 30f;

    public override void OnReadyAction(StateController controller)
    {
        controller.variables.shotsInRounds = Random.Range(controller.maximumBurst / 2, controller.maximumBurst);
        controller.variables.currentShots = 0;
        controller.variables.startShootTimer = 0f;
        controller.enemyAnimation.anim.ResetTrigger(AnimatorKey.Shooting);
        controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
        controller.variables.waitInCoverTime = 0;
        controller.enemyAnimation.ActivatePendingAim();//조준 대기. 이제 시야에만 들어오면 조준가능.
    }
    private void DoShot(StateController controller, Vector3 direction, Vector3 hitPoint, 
        Vector3 hitNormal = default, bool organic = false, Transform target = null)
    {
        GameObject muzzleFlash = EffectManager.Instance.EffectOneShot((int)EffectList.flash, Vector3.zero);
        muzzleFlash.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.left * 90f;
        DestroyDelayed destroyDelayed = muzzleFlash.AddComponent<DestroyDelayed>();
        destroyDelayed.DelayTime = 0.5f;//auto destroy.

        GameObject shotTracer = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, Vector3.zero);
        shotTracer.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        Vector3 origin = controller.enemyAnimation.gunMuzzle.position;
        shotTracer.transform.position = origin;
        shotTracer.transform.rotation = Quaternion.LookRotation(direction);

        if(target && !organic)
        {
            GameObject bulletHole = EffectManager.Instance.EffectOneShot((int)EffectList.bulletHole,
                hitPoint + 0.01f * hitNormal);
            bulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

            GameObject instantSpark = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, hitPoint);

        }else if(target && organic) //player
        {
            HealthBase targetHealth = target.GetComponent<HealthBase>();//playerHealth
            if(targetHealth)
            {
                targetHealth.TakeDamage(hitPoint, direction, controller.classStats.BulletDamage,
                    target.GetComponent<Collider>(), controller.gameObject);
            }
        }

        SoundManager.Instance.PlayShotSound(controller.classID, controller.enemyAnimation.gunMuzzle.position, 2f);
    }

    private void CastShot(StateController controller)
    {
        Vector3 imprecision =
            Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate) *
            controller.transform.right;
        imprecision += Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate) *
            controller.transform.up;
        Vector3 shotDirection = controller.personalTarget - controller.enemyAnimation.gunMuzzle.position;
        shotDirection = shotDirection.normalized + imprecision;
        Ray ray = new Ray(controller.enemyAnimation.gunMuzzle.position, shotDirection);
        if(Physics.Raycast(ray, out RaycastHit hit, controller.viewRadius, controller.generalStats.shotMask.value))
        {
            bool isOrganic = ((1 << hit.transform.root.gameObject.layer) & controller.generalStats.targetMask) != 0;
            DoShot(controller, ray.direction, hit.point, hit.normal, isOrganic, hit.transform);
        }
        else
        {
            DoShot(controller, ray.direction, ray.origin + (ray.direction * 500f));
        }
    }

    private bool CanShoot(StateController controller)
    {
        float distance = (controller.personalTarget -
            controller.enemyAnimation.gunMuzzle.position).sqrMagnitude;
        if(controller.Aiming && 
            (controller.enemyAnimation.currentAimingAngleGap <  aimAngleGap ||
            distance <= 5.0f))
        {
            if(controller.variables.startShootTimer >= startShootDelay)
            {
                return true;
            }else
            {
                controller.variables.startShootTimer += Time.deltaTime;
            }

        }
        return false;
    }
    private void Shoot(StateController controller)
    {
        if(Time.timeScale > 0 && controller.variables.shotTimer == 0f)
        {
            controller.enemyAnimation.anim.SetTrigger(AnimatorKey.Shooting);
            CastShot(controller);
        }
        else if(controller.variables.shotTimer >= (0.1f + 2f * Time.deltaTime))
        {
            controller.bullets = Mathf.Max(--controller.bullets, 0);
            controller.variables.currentShots++;
            controller.variables.shotTimer = 0;
            return;
        }
        controller.variables.shotTimer += controller.classStats.ShotRateFactor * Time.deltaTime;
    }
    public override void Act(StateController controller)
    {
        controller.focusSight = true;

        if(CanShoot(controller))
        {
            Shoot(controller);
        }
        controller.variables.blindEngageTimer += Time.deltaTime;
    }

}
