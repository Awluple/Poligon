using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.EvetArgs;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected Character character;

    protected void Awake() {
        animator = GetComponent<Animator>();
        character.OnHeavyLanding += HeavyLanding;
        character.OnHeavyLandingEnd += HeavyLandingEnd;
        character.OnFalling += Falling;
        character.OnAiming += StartAiming;
        character.OnAimingEnd += StopAiming;
        character.OnCrouching += StartCrouching;
        character.OnCrouchingEnd += StopCrouching;
        character.OnShoot += StartShooting;
        character.OnShootEnd += StopShooting;

        character.OnAimingWalk += AimWalk;


    }

    float yVelocity = 0.0f;
    [SerializeField] protected float aimTransitionTime = 0.2f;

    protected void Update() {
        animator.SetBool("isWalking", character.IsWalking());
        animator.SetBool("isRunning", character.IsRunning());

        if(character.isGrounded()) {
            animator.SetBool("falling", false);
            animator.SetBool("jumped", false);
        }

        if(!character.IsAiming() && animator.GetLayerWeight(2) > 0) {
            if(animator.GetLayerWeight(2) < 0.05) {
                animator.SetLayerWeight(2, 0);
            } else {
                var m_currentLayerWeight = animator.GetLayerWeight(2);
                m_currentLayerWeight = Mathf.SmoothDamp(m_currentLayerWeight, character.IsAiming() ? 1 : 0, ref yVelocity, aimTransitionTime);
                animator.SetLayerWeight(2, m_currentLayerWeight);
            }
            
        } else if (character.IsAiming() && animator.GetLayerWeight(2) < 1) {
            if (animator.GetLayerWeight(2) > 0.95) {
                animator.SetLayerWeight(2, 1);
            } else {
                var m_currentLayerWeight = animator.GetLayerWeight(2);
                m_currentLayerWeight = Mathf.SmoothDamp(m_currentLayerWeight, character.IsAiming() ? 1 : 0, ref yVelocity, aimTransitionTime);
                animator.SetLayerWeight(2, m_currentLayerWeight);
            }
        }

        //if(character.IsCrouching() && animator.GetLayerWeight(6) > 0) {
        //    animator.SetLayerWeight(2, 1);
        //}

     }
    protected void Falling(object sender, System.EventArgs e) {
        animator.SetBool("falling", true);
    }
    protected void HeavyLanding(object sender, System.EventArgs e) {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("heavyLanding", true);

        animator.SetLayerWeight(3, 1);
    }
    protected void HeavyLandingEnd(object sender, System.EventArgs e) {
        animator.SetBool("heavyLanding", false);
        animator.SetBool("falling", false);
        animator.SetBool("jumped", false);
        animator.SetLayerWeight(3, 0);
    }

    protected void StartAiming(object sender, System.EventArgs e) {
        if(character.IsCrouching()) {
            animator.SetLayerWeight(6, 1);
            animator.SetLayerWeight(5, 0);
        } else {
            animator.SetLayerWeight(4, 1);
        }
        animator.SetBool("aiming", true);
    }
    protected void StopAiming(object sender, System.EventArgs e) {
        if (character.IsCrouching()) {
            animator.SetLayerWeight(6, 0);
            animator.SetLayerWeight(5, 1);
        } else {
            animator.SetLayerWeight(4, 0);
        }
        animator.SetBool("aiming", false);
    }

    protected void StartShooting(object sender, System.EventArgs e) {
        //animator.SetBool("shoot", true);
        animator.SetTrigger("shoot");
    }
    protected void StopShooting(object sender, System.EventArgs e) {
        //animator.SetBool("shoot", false);
    }

    protected void AimWalk(object sender, Vector2EventArgs args) {
        animator.SetFloat("xMovement", args.Vector.x);
        animator.SetFloat("yMovement", args.Vector.y);
    }

    protected void StartCrouching(object sender, System.EventArgs e) {
        animator.SetLayerWeight(0, 0);

        if (character.IsAiming()) {
            animator.SetLayerWeight(6, 1);
            animator.SetLayerWeight(4, 0);
        } else {
            animator.SetLayerWeight(5, 1);
        }
        animator.SetBool("crouching", true);
    }
    protected void StopCrouching(object sender, System.EventArgs e) {
        animator.SetLayerWeight(0, 1);
        animator.SetLayerWeight(5, 0);

        if (character.IsAiming()) {
            animator.SetLayerWeight(4, 1);
            animator.SetLayerWeight(6, 0);
        }
        animator.SetBool("crouching", false);
    }
}
