using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/Exit Focus")]
public class ExitFocusAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.variables.feelAlert = false;
        controller.variables.hearAlert = false;
        controller.Strafing = false;
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = 0f;
    }
    public override void Act(StateController controller)
    {
        
    }
}
