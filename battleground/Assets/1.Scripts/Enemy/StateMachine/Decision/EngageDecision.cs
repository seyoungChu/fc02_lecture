using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 타겟이 보이거나 근처에 있으면 교전 대기 시간을 초기화하고
/// 반대로 보이지 않거나 멀어져 있거나하면 blindEngageTime 만큼 기다릴걸지///
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Engage")]
public class EngageDecision : Decision
{
    [Header("Extra Decision")]
    public LookDecision isViewing;
    public FocusDecision targetNear;

    public override bool Decide(StateController controller)
    {
        if(isViewing.Decide(controller) || targetNear.Decide(controller))
        {
            controller.variables.blindEngageTimer = 0;
        }
        else if(controller.variables.blindEngageTimer >= controller.blindEngageTime)
        {
            controller.variables.blindEngageTimer = 0;
            return false;
        }
        return true;
    }

}
