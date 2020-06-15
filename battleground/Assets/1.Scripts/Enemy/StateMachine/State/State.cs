using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{
    public Action[] actions;
    public Transition[] transitions;
    public Color sceneGizmoColor = Color.gray;

    public void DoActions(StateController controller)
    {
        for(int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(controller);
        }
    }
    public void OnEnableActions(StateController controller)
    {
        for(int i = 0; i < actions.Length;i++)
        {
            actions[i].OnReadyAction(controller);
        }
        for(int i = transitions.Length - 1; i >= 0;i--)
        {
            transitions[i].decision.OnEnableDecision(controller);
        }
    }
    public void CheckTransitions(StateController controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool decision = transitions[i].decision.Decide(controller);
            if(decision)
            {
                controller.TransitionToState(transitions[i].trueState, transitions[i].decision);
            }
            else
            {
                controller.TransitionToState(transitions[i].falseState, transitions[i].decision);
            }
            if(controller.currentState != this)
            {
                controller.currentState.OnEnableActions(controller);
                break;
            }
        }

    }
}
