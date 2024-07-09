using Poligon.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolAim : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if ((WeaponTypes)animator.GetInteger("currentWeapon") == WeaponTypes.Pistol && !animator.GetBool("aiming") && !animator.GetBool("crouching")) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 0);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.PistolUp, 1);
        } else if((WeaponTypes)animator.GetInteger("currentWeapon") != WeaponTypes.Pistol) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 0);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.PistolUp, 0);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
