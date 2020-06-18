using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 장애물로 이동할수 있는 상황인지 아닌지 판단..
/// 쏴야할 총알이 남아 있거나, 엄폐물로 이동하기 전에 대기 시간이 남아 있거나,
/// 만약에 숨을만한 엄폐물이 없을 겨우는 False,
/// 그외에는 그냥 엄폐물로 이동. true
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Take Cover")]
public class TakeCoverDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        //지금 쏴야할 총알이 남아 있거나, 대기 시간이 더 필요하거나, 엄폐물 위치를 못찾앗따면 false
        if (controller.variables.currentShots < controller.variables.shotsInRounds ||
            controller.variables.waitInCoverTime > controller.variables.coverTime ||
            Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
