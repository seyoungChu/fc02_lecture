using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/GotoShot Spot")]
public class GotoShotSpotAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = controller.generalStats.chaseSpeed;
        controller.enemyAnimation.AbortPendingAim();
    }
    public override void Act(StateController controller)
    {
        
    }
}
