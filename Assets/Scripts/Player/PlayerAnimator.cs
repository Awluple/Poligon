using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharacterAnimator {
    private Player player;

    protected new void Awake() {
        base.Awake();
        try {
            player = (Player)character;
        } catch (InvalidCastException) {
            Debug.LogError($"The character '{character.gameObject.name}' does not implement the Player class");
        }
        player.OnJumpStart += JumpStart;
        player.OnJumpEnd += JumpEnd;
    }

    protected void JumpStart(object sender, System.EventArgs e) {
        animator.SetBool("jumped", true);
    }
    protected void JumpEnd(object sender, System.EventArgs e) {
        animator.SetBool("falling", false);
        animator.SetBool("jumped", false);
    }
}
