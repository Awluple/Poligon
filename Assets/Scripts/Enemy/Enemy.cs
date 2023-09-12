using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Poligon.Extensions;
using TMPro;
using Poligon.EvetArgs;

public class Enemy : Character {


    [SerializeField] private EnemyController enemyController;


    void Awake() {
        isRunning = true;
        characterController = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start() {
        enemyController.OnRunStart += StartRun;
        enemyController.OnRunCancel += CancelRun;

        enemyController.OnAimStart += AimStart;
        enemyController.OnAimCancel += AimCancel;


        enemyController.OnShootPerformed += ShootPerformed;
        enemyController.OnShootCancel += ShootCancel;


        characterController = GetComponent<CharacterController>();

    }

    public EnemyAimPosition getAimPosition() {
        return (EnemyAimPosition)aimingTarget;
    }


    // Update is called once per frame
    void Update() {
        if (!stunned) {
            Move();
            }// else {
             //    movement.x = 0;
             //    movement.z = 0;
             //}

        movement.y += Physics.gravity.y * fallingGravityStrength * Time.deltaTime;

        //if (isWalking == false) {
        //    isRunning = false;
        //}
        //if (characterController.isGrounded && jumped == false) {
        //    movement.y = -1f;
        //    characterController.stepOffset = originalStepOffset;
        //}

        if ((isWalking && !stunned) || isAiming) {
            Rotate(new Vector3(movement.x, 0f, movement.z));
        }
        characterController.Move(movement * Time.deltaTime);

        //int layer_mask = LayerMask.GetMask("Ground");
        //bool hit = false;

        //if (!characterController.isGrounded) {
        //    hit = Physics.Raycast(transform.position, Vector3.down, 2f, layer_mask);
        //}

        //if (hit && movement.y < 0f) {
        //    if (movement.y <= -25f) {
        //        stunned = true;
        //        StartCoroutine(HeavyFallStun());
        //    }
        //} else if (!characterController.isGrounded && !hit && !(movement.y >= 0f)) {
        //    if (OnFalling != null) OnFalling(this, EventArgs.Empty);
        //}

    }
    protected override void Rotate(Vector3 moveDir) {
        Quaternion? toRotation;
        if (!isAiming) {
            toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        } else {
            Vector3 target = aimingTarget.GetPosition();
            target.y = transform.position.y;
            toRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        }
        if (toRotation != null) transform.rotation = Quaternion.RotateTowards(transform.rotation, (Quaternion)toRotation, rotationSpeed * Time.deltaTime);
    }

    protected override void Move() {
        Vector2 inputVector = enemyController.GetMovementVectorNormalized();
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

        if (aimBlendTree != Vector2.zero) {
            if (aimBlendTree.y < -0.35f || Mathf.Abs(aimBlendTree.x) > 0.35f) {
                speed = speed - speed * 0.3f;
            }
        }

        movement.x = inputVector.x * speed;
        movement.z = inputVector.y * speed;

    }
}