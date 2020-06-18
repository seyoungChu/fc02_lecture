using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Return To Cover")]
public class ReturnToCoverAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        if(!Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            controller.nav.destination = controller.CoverSpot;
            controller.nav.speed = controller.generalStats.chaseSpeed;
            if (Vector3.Distance(controller.CoverSpot, controller.transform.position) > 0.5f)
            {
                controller.enemyAnimation.AbortPendingAim();
            }
        }
        else
        {
            controller.nav.destination = controller.transform.position;
        }
        
    }
    public override void Act(StateController controller)
    {
        if (!Equals(controller.CoverSpot, controller.transform.position))
        {
            controller.focusSight = false;
        }
    }
}
