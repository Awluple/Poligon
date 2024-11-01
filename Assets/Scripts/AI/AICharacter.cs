using Poligon.Ai;
using Poligon.Enums;
using Poligon.EvetArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public class AICharacter : Character, ISquadMember {


    [SerializeField] private AiCharacterController enemyController;


    public Squad squad { get; set; }

    // Start is called before the first frame update
    protected override void Awake() {
        base.Awake();
        enemyController.OnRunStart += StartRun;
        enemyController.OnRunCancel += CancelRun;

        enemyController.OnAimStart += AimStart;
        enemyController.OnAimCancel += AimCancel;

        enemyController.OnShootPerformed += ShootPerformed;
        enemyController.OnShootCancel += ShootCancel;

        enemyController.OnCrouchStart += StartCrouch;
        enemyController.OnCrouchCancel += CancelCrouch;

        enemyController.OnReloadStart += Reload;
        enemyController.OnChangeWeapon += ChangeWeapon;

        characterController = GetComponent<CharacterController>();

    }
    private void Start() {
        if (currentWeapon == WeaponTypes.None) {
            ChangeWeapon(null, new InputValueEventArgs((int)WeaponTypes.AssultRifle));
        }
    }

    public EnemyAimPosition GetAimPosition() {
        return (EnemyAimPosition)aimingTarget;
    }
    public AiCharacterController GetController() {
        return enemyController;
    }


    // Update is called once per frame
    protected override void Update() {
        base.Update();
        if (!stunned && health > 0) {
            Move();
        }

        movement.y += Physics.gravity.y * fallingGravityStrength * Time.deltaTime;

        //if (isWalking == false) {
        //    isRunning = false;
        //}


        if (health > 0 && ((isWalking && !stunned) || isAiming)) {
            //Rotate(new Vector3(movement.x, 0f, movement.z));
            //Rotate(aimingTarget.transform.position);
        }
        if(isAiming && !GetAimPosition().destroyCancellationToken.IsCancellationRequested) {
            var turnTowardNavSteeringTarget = enemyController.navAgent.steeringTarget;
            transform.rotation = Quaternion.Slerp(transform.rotation, GetRotation(GetAimPosition().transform.position), rotationSpeed * Time.deltaTime);
        }

    }

    public void RotateSelf(Vector3 direction) {
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotationSpeed * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
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
    protected Quaternion GetRotation(Vector3 moveDir) {
        Quaternion? toRotation;
        if (!isAiming) {
            toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        } else {
            Vector3 target = aimingTarget.GetPosition();
            target.y = transform.position.y;
            toRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        }
        return (Quaternion)toRotation;
    }

    protected override void Move() {
        Vector2 velocity = new Vector2(enemyController.navAgent.velocity.x, enemyController.navAgent.velocity.z);
        //Vector2 inputVector = enemyController.GetMovementVectorNormalized();
        enemyController.GetMovementVectorNormalized();
        Vector2 inputVector = velocity.normalized;
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
        enemyController.navAgent.speed = speed;


        //movement.x = inputVector.x * speed;
        //movement.z = inputVector.y * speed;
    }
}
