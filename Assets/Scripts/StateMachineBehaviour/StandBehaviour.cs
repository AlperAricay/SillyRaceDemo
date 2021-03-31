using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class StandBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.GetComponent<IRunner>().IsStanding = true;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.GetComponent<IRunner>().IsStanding = false;
    }
}
