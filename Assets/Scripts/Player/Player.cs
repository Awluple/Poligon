using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Poligon.Extensions;
using TMPro;
using Poligon.EvetArgs;

public class Player : Character {


    [SerializeField] private float jumpStrength = 14f;
    [SerializeField] private float jumpGracePeriod = 0.1f;

    private float originalStepOffset;
    private float? lastGroundedTime;
    private bool jumped = false;

    [SerializeField] private GameInput gameInput;

    public event EventHandler OnJumpStart;
    public event EventHandler OnJumpEnd;


    // Start is called before the first frame update
    void Start() {
        gameInput.OnRunStart += StartRun;
        gameInput.OnRunCancel += CancelRun;

        gameInput.OnJumpStart += Jump;

        gameInput.OnAimStart += AimStart;
        gameInput.OnAimCancel += AimCancel;


        gameInput.OnShootPerformed += ShootPerformed;
        gameInput.OnShootCancel += ShootCancel;


        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
        this.health = 10000;

    }


    // Update is called once per frame
    void Update() {
        if(!stunned) {
            Move();
        } else {
            movement.x = 0;
            movement.z = 0;
        }

        movement.y += Physics.gravity.y * fallingGravityStrength * Time.deltaTime;

        if (isWalking == false) {
            isRunning = false;
        }

        if (characterController.isGrounded) {
            lastGroundedTime = Time.time;
        }
        if (characterController.isGrounded && jumped == false) {
            movement.y = -1f;
            characterController.stepOffset = originalStepOffset;
        }

        if ((isWalking && !stunned) || isAiming) {
            Rotate(new Vector3(movement.x, 0f, movement.z));
        }

        characterController.Move(movement * Time.deltaTime);
        int layer_mask = LayerMask.GetMask("Ground");
        bool hit = false;

        if(!characterController.isGrounded) {
            hit = Physics.Raycast(transform.position, Vector3.down, 2f, layer_mask);
        }

        if (hit && movement.y < 0f) {
            if (movement.y <= -25f) {
                stunned = true;
                StartCoroutine(HeavyFallStun());
            }
        } else if(!characterController.isGrounded && !hit && !(movement.y >= 0f)) {
            if (OnFalling != null) OnFalling(this, EventArgs.Empty);
        }

    }
    private void Jump(object sender, System.EventArgs e) {

        if (!stunned && Time.time - lastGroundedTime <= jumpGracePeriod) { // allow jumping in air if touched ground in last 'jumpGracePeriod' seconds
            if (OnJumpStart != null) OnJumpStart(this, EventArgs.Empty);
            characterController.stepOffset = 0;
            movement.y = jumpStrength;
            jumped = true;
        }
    }

    private void LateUpdate() {
        jumped = false;
    }

    protected override void Move() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        isWalking = inputVector.x != 0 || inputVector.y != 0;
        Vector2 aimBlendTree = Vector2.zero;

        if (IsAiming() && !isWalking) {
            if (OnAimingWalk != null) OnAimingWalk(this, new Vector2EventArgs(new Vector2(0, 0)));
        }
        if (IsAiming() && isWalking) {
            aimBlendTree = GetAimWalkBlendTreeValues(inputVector);

            if (OnAimingWalk != null) OnAimingWalk(this, new Vector2EventArgs(aimBlendTree));
        }

        float speed = IsRunning() == true ? sprintSpeed :
                      isAiming == true ? aimSpeed :
                      moveSpeed;
        
        if(aimBlendTree != Vector2.zero) {
            if(aimBlendTree.y < -0.35f || Mathf.Abs(aimBlendTree.x) > 0.35f) {
                speed = speed - speed * 0.3f;
            }
        }

        movement.x = inputVector.x * speed;
        movement.z = inputVector.y * speed;

    }
}
