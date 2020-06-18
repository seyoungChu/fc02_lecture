using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.enemyAnimation.AbortPendingAim();
        controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
        controller.personalTarget = Vector3.positiveInfinity;
        controller.CoverSpot = Vector3.positiveInfinity;
    }

    private void Patrol(StateController controller)
    {
        if (controller.patrolWaypoints.Count == 0)
        {
            return;
        }

        controller.focusSight = false;
        controller.nav.speed = controller.generalStats.patrolSpeed;
        if (controller.nav.remainingDistance <= controller.nav.stoppingDistance && !controller.nav.pathPending)
        {
            controller.variables.patrolTimer += Time.deltaTime;
            if (controller.variables.patrolTimer >= controller.generalStats.patrolWaitTime)
            {
                controller.wayPointIndex = (controller.wayPointIndex + 1) % controller.patrolWaypoints.Count;
                controller.variables.patrolTimer = 0;
            }
        }
        try
        {
            controller.nav.destination = controller.patrolWaypoints[controller.wayPointIndex].position;
        }
        catch (UnassignedReferenceException)
        {
            Debug.LogWarning("웨이포인트가 없어요 세팅해주세요.", controller.gameObject);
            controller.patrolWaypoints = new List<Transform>
            {
                controller.transform
            };
            controller.nav.destination = controller.transform.position;
        }
    }

    public override void Act(StateController controller)
    {
        Patrol(controller);
    }

}
