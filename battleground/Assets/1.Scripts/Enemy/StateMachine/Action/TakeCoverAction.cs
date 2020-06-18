using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="PluggableAI/Actions/Take Cover")]
public class TakeCoverAction : Action
{
    private readonly int coverMin = 2;
    private readonly int coverMax = 5;

    public override void OnReadyAction(StateController controller)
    {
        controller.variables.feelAlert = false;
        controller.variables.waitInCoverTime = 0f;
        if(!Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, true);
            controller.variables.coverTime = Random.Range(coverMin, coverMax);
        }else
        {
            controller.variables.coverTime = 0.1f;
        }
    }
    private void Rotating(StateController controller)
    {
        Vector3 dirToVector = controller.personalTarget - controller.transform.position;
        if(dirToVector.sqrMagnitude < 0.001f || dirToVector.sqrMagnitude > 1000000.0f)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(dirToVector);
        if(Quaternion.Angle(controller.transform.rotation, targetRotation) > 5f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation,
                10f * Time.deltaTime);
        }
    }
    public override void Act(StateController controller)
    {
        if(!controller.reloading)
        {
            controller.variables.waitInCoverTime += Time.deltaTime;
        }
        controller.variables.blindEngageTimer += Time.deltaTime;
        if(controller.enemyAnimation.anim.GetBool(AnimatorKey.Crouch))
        {
            Rotating(controller);
        }
    }
}
