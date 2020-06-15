using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 실제 행동을 하게 되는 컴포넌트.
/// </summary>
public abstract class Action : ScriptableObject
{
    public abstract void Act(StateController controller);

    public virtual void OnReadyAction(StateController controller)
    {

    }
}
