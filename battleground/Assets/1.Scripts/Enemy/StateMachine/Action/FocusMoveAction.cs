using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 공격과 동시에 이동하는 액션이며, 일단 회전할때는 회전을 하고 회전을 다했으면
/// strafing이 활성화 됩니다.
/// </summary>
/// 
[CreateAssetMenu(menuName ="PluggableAI/Actions/Focus Move")]
public class FocusMoveAction : Action
{
    public ClearShotDecision clearShotDecision;

    private Vector3 currentDest; //
    private bool aligned;

    public override void OnReadyAction(StateController controller)
    {
        controller.hadClearShot = controller.haveClearShot = false;
        currentDest = controller.nav.destination;
        controller.focusSight = true;
        aligned = false;
    }
    public override void Act(StateController controller)
    {
        if (!aligned)
        {
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = 0f;
            if(controller.enemyAnimation.angularSpeed == 0f)
            {
                controller.Strafing = true;
                aligned = true;
                controller.nav.destination = currentDest;
                controller.nav.speed = controller.generalStats.evadeSpeed;
            }

        } else
        {
            controller.haveClearShot = clearShotDecision.Decide(controller);
            if(controller.hadClearShot != controller.haveClearShot)
            {
                controller.Aiming = controller.haveClearShot;
                //사격이 가능하다면 현재 이동 목표가 엄폐물과 다르더라도 일단 이동하지 말아라
                if(controller.haveClearShot && !Equals(currentDest, controller.CoverSpot))
                {
                    controller.nav.destination = controller.transform.position;
                }
            }
            controller.hadClearShot = controller.haveClearShot;
        }
    }
}
