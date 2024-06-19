using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.EvetArgs;
using Poligon.Enums;


public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected Character character;
    [SerializeField] protected AvatarMask avatarMask;

    private int leaningPosition = 0;

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

        character.OnLeaning += StartLeaning;
        character.OnLeaningEnd += StopLeaning;
        character.OnMoving += StopLeaningOnMove;
        character.OnMovingEnd += ResumeLeaning;

        character.OnWeaponChange += ChangeWeapon;

        character.OnReload += StartReload;
        character.OnReloadEnd += EndReload;
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

        //if(!character.IsAiming() && animator.GetLayerWeight(2) > 0) {
        //    if(animator.GetLayerWeight((int)CharacterAnimatorLayers.Pistol) < 0.01) {
        //        animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 0);
        //    } else {
        //        var m_currentLayerWeight = animator.GetLayerWeight((int)CharacterAnimatorLayers.Pistol);
        //        m_currentLayerWeight = Mathf.SmoothDamp(m_currentLayerWeight, character.IsAiming() ? 1 : 0, ref yVelocity, aimTransitionTime);
        //        animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, m_currentLayerWeight);
        //    }
            
        //} else if (character.IsAiming() && animator.GetLayerWeight((int)CharacterAnimatorLayers.Pistol) < 1) {
        //    if (animator.GetLayerWeight((int)CharacterAnimatorLayers.Pistol) > 0.99) {
        //        animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 1);
        //    } else {
        //        var m_currentLayerWeight = animator.GetLayerWeight((int)CharacterAnimatorLayers.Pistol);
        //        m_currentLayerWeight = Mathf.SmoothDamp(m_currentLayerWeight, character.IsAiming() ? 1 : 0, ref yVelocity, aimTransitionTime);
        //        animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, m_currentLayerWeight);
        //    }
        //}

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

        animator.SetLayerWeight((int)CharacterAnimatorLayers.HeavyLanding, 1);
    }
    protected void HeavyLandingEnd(object sender, System.EventArgs e) {
        animator.SetBool("heavyLanding", false);
        animator.SetBool("falling", false);
        animator.SetBool("jumped", false);
        animator.SetLayerWeight((int)CharacterAnimatorLayers.HeavyLanding, 0);
    }

    protected void StartAiming(object sender, System.EventArgs e) {
        if(character.IsCrouching()) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.CrouchingBlend, 1);
            animator.SetBool("crouching", true);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.Crouching, 0);

            if (character.currentWeapon == WeaponTypes.AssultRifle) {
                avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            }

        } else {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.WalkingBlend, 1);
        }
        animator.SetBool("aiming", true);
    }
    protected void StopAiming(object sender, System.EventArgs e) {
        if (character.IsCrouching()) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.CrouchingBlend, 0);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.Crouching, 1);
            animator.SetBool("crouching", true);
        } else {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.WalkingBlend, 0);
        }
        avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Leaning, 0);
        animator.SetBool("aiming", false);
    }

    protected void StartShooting(object sender, System.EventArgs e) {
        //animator.SetBool("shoot", true);
        animator.SetTrigger("shoot");
    }
    protected void StopShooting(object sender, System.EventArgs e) {
        //animator.SetBool("shoot", false);
    }

    protected void StartLeaning(object sender, InputValueEventArgs args) {
        if (!character.IsAiming() || character.IsWalking()) {
            leaningPosition = (int)args.Value;
            return;
        };
        leaningPosition = (int)args.Value;
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Leaning, 1);
        animator.SetInteger("leanState", (int)args.Value);
    }

    protected void ResumeLeaning(object sender, System.EventArgs args) {
        if(leaningPosition != 0) StartLeaning(null, new InputValueEventArgs(leaningPosition));
    }

    protected void StopLeaning(object sender, System.EventArgs args) {
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Leaning, 0);
        animator.SetInteger("leanState", 0);
        leaningPosition = 0;
    }
    protected void StopLeaningOnMove(object sender, System.EventArgs args) {
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Leaning, 0);
        animator.SetInteger("leanState", 0);
    }

    protected void AimWalk(object sender, Vector2EventArgs args) {
        animator.SetFloat("xMovement", args.Vector.x);
        animator.SetFloat("yMovement", args.Vector.y);
    }

    protected void StartCrouching(object sender, System.EventArgs e) {
        animator.SetLayerWeight((int)CharacterAnimatorLayers.BaseLayer, 0);

        if (character.IsAiming()) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.CrouchingBlend, 1);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.WalkingBlend, 0);
        } else {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.Crouching, 1);
        }
        if(character.currentWeapon == WeaponTypes.AssultRifle) {
            avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, false);
            avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, false);
        }
        animator.SetBool("crouching", true);
        animator.SetTrigger("positionChange");
    }
    protected void StopCrouching(object sender, System.EventArgs e) {
        animator.SetLayerWeight((int)CharacterAnimatorLayers.BaseLayer, 1);
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Crouching, 0);
        animator.SetTrigger("positionChange");

        if (character.IsAiming()) {
            animator.SetLayerWeight((int)CharacterAnimatorLayers.WalkingBlend, 1);
            animator.SetLayerWeight((int)CharacterAnimatorLayers.CrouchingBlend, 0);
        }
        avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
        avatarMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
        animator.SetBool("crouching", false);
    }

    protected void ChangeWeapon(object sender, InputValueEventArgs e) {
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 0);
        animator.SetLayerWeight((int)CharacterAnimatorLayers.Rifle, 0);

        switch((WeaponTypes)e.Value) {
            case (WeaponTypes.Pistol):
                animator.SetLayerWeight((int)CharacterAnimatorLayers.Pistol, 1);
                break;
            case (WeaponTypes.AssultRifle):
                animator.SetLayerWeight((int)CharacterAnimatorLayers.Rifle, 1);
                break;
            default:
                Debug.LogWarning($"Animations for {(WeaponTypes)e.Value} are not present");
                break;
        }
    }
    protected void StartReload(object sender, System.EventArgs e) {
        animator.SetBool("isReloading", true);
    }
    protected void EndReload(object sender, System.EventArgs e) {
        animator.SetBool("isReloading", false);
    }
}
