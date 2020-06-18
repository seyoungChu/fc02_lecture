using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 숨을 수 있는 엄폐물이 없다면 가만히 서있지만 새로운 엄폐물이 있고 엄폐물보다 가깝다면 엄폐물을 변경.
/// 총알 장전도 해준다.
/// </summary>
/// 
[CreateAssetMenu(menuName ="PluggableAI/Actions/FindCover")]
public class FindCoverAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.enemyAnimation.AbortPendingAim();
        controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
        ArrayList nextCoverData = controller.coverLookUp.GetBestCoverSpot(controller);
        Vector3 potentialCover = (Vector3)nextCoverData[1];
        if(Vector3.Equals(potentialCover, Vector3.positiveInfinity))
        {
            controller.nav.destination = controller.transform.position;
            return;
        }
        else if( (controller.personalTarget - potentialCover).sqrMagnitude < 
            (controller.personalTarget - controller.CoverSpot).sqrMagnitude &&
            !controller.IsNearOtherSpot(potentialCover, controller.nearRadius))
        {
            controller.coverHash = (int)nextCoverData[0];
            controller.CoverSpot = potentialCover;
        }
        controller.nav.destination = controller.CoverSpot;
        controller.nav.speed = controller.generalStats.evadeSpeed;

        controller.variables.currentShots = controller.variables.shotsInRounds;                       
    }
    public override void Act(StateController controller)
    {
        
    }
}
