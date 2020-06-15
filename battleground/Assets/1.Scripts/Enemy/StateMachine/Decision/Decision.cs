using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 조건 체크하는 클래스.
/// 조건 체크를 위해 특정 위치로 부터 원하는 검색 반경에 있는 충돌체를 찾아서 그 안에 타겟이 있는지 확인
/// </summary>
public abstract class Decision : ScriptableObject
{
    public abstract bool Decide(StateController controller);
    
    public virtual void OnEnableDecision(StateController controller)
    {

    }
    public delegate bool HandleTargets(StateController controller, bool hasTargets, 
        Collider[] targetInRadius);
    public static bool CheckTargetsInRadius(StateController controller, float radius, HandleTargets
        handleTargets)
    {
        if(controller.aimTarget.root.GetComponent<HealthBase>().IsDead)
        {
            return false;
        }
        else
        {
            Collider[] targetsInRadius =
                Physics.OverlapSphere(controller.transform.position, radius,
                controller.generalStats.targetMask);
            return handleTargets(controller, targetsInRadius.Length > 0, targetsInRadius);
        }
    }
}
