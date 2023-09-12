using Poligon.EvetArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class Character : MonoBehaviour, IKillable {
    [SerializeField] protected Transform groundCheck;

    [SerializeField] protected float moveSpeed = 3.25f;
    [SerializeField] protected float sprintSpeed = 10f;
    [SerializeField] protected float aimSpeed = 2f;

    [SerializeField] protected float rotationSpeed = 720f;

    [SerializeField] protected float fallingGravityStrength = 5f;

    [SerializeField] protected float heavyFallStunTime = 3.1f;


    [SerializeField] protected Rig rig;
    [SerializeField] protected Gun gun;
    [SerializeField] protected AimPosition aimingTarget;

    protected Vector3 movement = Vector3.zero;

    protected CharacterController characterController;
    protected bool isWalking;
    protected bool isRunning;
    protected bool isAiming;
    protected bool stunned = false;

    [SerializeField] protected float health = 100;


    //public event EventHandler OnFalling;
    public EventHandler OnFalling { get; set; }

    event EventHandler Falling {
        add => OnFalling += value;
        remove => OnFalling -= value;
    }

    public event EventHandler OnHeavyLanding;
    public event EventHandler OnHeavyLandingEnd;
    public event EventHandler OnAiming;
    public event EventHandler OnAimingEnd;
    public event EventHandler OnShoot;
    public event EventHandler OnShootEnd;

    public Vector2EventHandler OnAimingWalk { get; set; }

    event Vector2EventHandler AimingWalk {
        add => OnAimingWalk += value;
        remove => OnAimingWalk -= value;
    }


    //public event Vector2EventHandler OnAimingWalk;

    public delegate void Vector2EventHandler(object sender, Vector2EventArgs args);

    protected abstract void Move();


    protected void StartRun(object sender, System.EventArgs e) {
        if (isAiming != true) {
            isRunning = true;
        }
    }
    protected void CancelRun(object sender, System.EventArgs e) {
        isRunning = false;
    }

    protected void AimStart(object sender, System.EventArgs e) {
        if (isGrounded() && !stunned) {

            if (OnAiming != null) OnAiming(this, EventArgs.Empty);
            isAiming = true;
            isRunning = false;
            rig.weight = 1;
        }
    }

    protected void AimCancel(object sender = null, System.EventArgs e = null) {
        if (!isAiming) return;
        rig.weight = 0;
        if (OnAimingEnd != null) OnAimingEnd(this, EventArgs.Empty);
        isAiming = false;
    }

    protected void ShootPerformed(object sender, System.EventArgs e) {
        if (!isAiming) return;

        if (OnShoot != null && gun.CanShoot()) OnShoot(this, EventArgs.Empty);
        gun.Shoot();
    }

    protected void ShootCancel(object sender, System.EventArgs e) {
        if (!isAiming) return;
        if (OnShootEnd != null) OnShootEnd(this, EventArgs.Empty);
    }

    protected IEnumerator HeavyFallStun() {
        isWalking = false;
        isRunning = false;
        rig.weight = 0;
        AimCancel();
        if (OnHeavyLanding != null) OnHeavyLanding(this, EventArgs.Empty);

        yield return new WaitForSeconds(heavyFallStunTime);

        stunned = false;


        if (OnHeavyLandingEnd != null) OnHeavyLandingEnd(this, EventArgs.Empty);
    }
    public void ApplyDamage(float damage) {
        this.health -= damage;

        if(this.health <= 0) {
            Destroy(gameObject);
        }
    }


    protected virtual void Rotate(Vector3 moveDir) {

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

    protected Vector2 GetAimWalkBlendTreeValues(Vector2 inputVector) {
        var move_Angle = Vector2.Angle(inputVector, new Vector2(aimingTarget.GetPosition().x - transform.position.x, aimingTarget.GetPosition().z - transform.position.z));
        if (move_Angle < 0) {

            move_Angle = 360 + move_Angle;
        }

        float radians = (Mathf.PI / 180) * move_Angle;
        float x = 1f * Mathf.Sin(radians);
        float y = 1f * Mathf.Cos(radians);

        int xVal = inputVector.x < 0 ? -1 : 1;
        int yVal = inputVector.y < 0 ? -1 : 1;

        x = x * xVal * yVal;

        if (aimingTarget.GetTransform().eulerAngles.y > 45 && aimingTarget.GetTransform().eulerAngles.y < 225) {
            x = -x;
        }
        return new Vector2(x, y);
    }

    public bool isGrounded() {
        return true;
    }
    public bool IsWalking() {
        return isWalking;
    }
    public bool IsRunning() {
        return isRunning;
    }
    public bool IsAiming() {
        return isAiming;
    }

}
