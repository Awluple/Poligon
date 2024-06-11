using Poligon.Enums;
using Poligon.EvetArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] protected Gun gun = null;
    public WeaponTypes currentWeapon = WeaponTypes.None;
    [SerializeField] protected AimPosition aimingTarget;

    protected Vector3 movement = Vector3.zero;

    protected CharacterController characterController;
    [SerializeField] protected bool isWalking;
    [SerializeField] protected bool isReloading;
    [SerializeField] protected bool isRunning;
    [SerializeField] protected bool isAiming;
    [SerializeField] protected bool isCrouching;
    [SerializeField] protected bool stunned = false;

    private bool lastFrameMoving = false;

    [SerializeField] protected float health = 100;

    private IEnumerator shootingCoroutine = null;


    //public event EventHandler OnFalling;
    public EventHandler OnFalling { get; set; }

    event EventHandler Falling {
        add => OnFalling += value;
        remove => OnFalling -= value;
    }

    public event EventHandler OnMoving;
    public event EventHandler OnMovingEnd;
    public event EventHandler OnHeavyLanding;
    public event EventHandler OnHeavyLandingEnd;
    public event EventHandler OnAiming;
    public event EventHandler OnAimingEnd;
    public event EventHandler OnCrouching;
    public event EventHandler OnCrouchingEnd;
    public event EventHandler OnShoot;
    public event EventHandler OnShootEnd;
    public event EventHandler OnReload;

    public event EventHandler<InputValueEventArgs> OnLeaning;
    public event EventHandler<InputValueEventArgs> OnLeaningEnd;
    public event EventHandler<InputValueEventArgs> OnWeaponChange;

    public event EventHandler OnDeath;
    public event EventHandler<BulletDataEventArgs> OnHealthLoss;

    public Vector2EventHandler OnAimingWalk { get; set; }

    event Vector2EventHandler AimingWalk {
        add => OnAimingWalk += value;
        remove => OnAimingWalk -= value;
    }

    protected virtual void Update() {
        if ((isWalking || isRunning) && !lastFrameMoving) {
            if (OnMoving != null) OnMoving(this, EventArgs.Empty);
        } else if (!(isWalking || isRunning) && lastFrameMoving) {
            if (OnMovingEnd != null) OnMovingEnd(this, EventArgs.Empty);
        }
        lastFrameMoving = isWalking || isRunning;
    }

    //public event Vector2EventHandler OnAimingWalk;

    private IEnumerator ShootingCoroutine() {
        while (gun.currentAmmo > 0) {
            if (OnShoot != null) OnShoot(this, EventArgs.Empty);
            gun.Shoot();
            gun.currentAmmo -= 1;
            yield return new WaitForSeconds(gun.fireRate);
        }
    }
    private IEnumerator ExecuteAfterTime(float time, Action task) {
        yield return new WaitForSeconds(time);

        task();
    }

    public delegate void Vector2EventHandler(object sender, Vector2EventArgs args);

    protected abstract void Move();


    protected void StartRun(object sender, System.EventArgs e) {
        if (isAiming != true && isCrouching != true) {
            isRunning = true;
        }
    }
    protected void CancelRun(object sender, System.EventArgs e) {
        isRunning = false;
    }

    protected void StartCrouch(object sender, System.EventArgs e) {
        isCrouching = true;
        isRunning = false;
        if (OnCrouching != null) OnCrouching(this, EventArgs.Empty);

    }
    protected void CancelCrouch(object sender, System.EventArgs e) {
        isCrouching = false;
        if (OnCrouchingEnd != null) OnCrouchingEnd(this, EventArgs.Empty);
    }

    protected void AimStart(object sender, System.EventArgs e) {
        if(currentWeapon == WeaponTypes.None) {
            return;
        }
        if (isGrounded() && !stunned) {

            if (OnAiming != null) OnAiming(this, EventArgs.Empty);
            isAiming = true;
            isRunning = false;
            rig.weight = 1;
        }
        switch (currentWeapon) {
            case (WeaponTypes.Pistol):
                
                break;
            case (WeaponTypes.AssultRifle):
                gun.transform.position = gun.equippedPositions[1].transform.position;
                gun.transform.rotation = gun.equippedPositions[1].transform.rotation;
                gun.transform.parent = gun.equippedPositions[1].transform.parent;
                break;
        }
    }

    protected void AimCancel(object sender = null, System.EventArgs e = null) {
        if (!isAiming) return;
        rig.weight = 0;
        if (OnAimingEnd != null) OnAimingEnd(this, EventArgs.Empty);
        isAiming = false;

        switch (currentWeapon) {
            case (WeaponTypes.Pistol):

                break;
            case (WeaponTypes.AssultRifle):
                gun.transform.position = gun.equippedPositions[0].transform.position;
                gun.transform.rotation = gun.equippedPositions[0].transform.rotation;
                gun.transform.parent = gun.equippedPositions[0].transform.parent;
                break;
        }
    }

    protected void ShootPerformed(object sender, System.EventArgs e) {
        if (!isAiming) return;
        if (gun.currentAmmo == 0) return;

        if(!gun.automatic) {
            if (OnShoot != null && gun.CanShoot()) {
                OnShoot(this, EventArgs.Empty);
                gun.Shoot();
                gun.currentAmmo -= 1;
            }
        } else {
            shootingCoroutine = ShootingCoroutine();
            StartCoroutine(shootingCoroutine);
        }
    }

    protected void ShootCancel(object sender, System.EventArgs e) {
        if (!isAiming) return;
        if (OnShootEnd != null) OnShootEnd(this, EventArgs.Empty);
        if(shootingCoroutine != null) {
            StopCoroutine(shootingCoroutine);
        }
    }

    protected void LeaningStart(object sender, InputValueEventArgs args) {
        if (OnLeaning != null) OnLeaning(this, args);
    }

    protected void LeaningCancel(object sender, InputValueEventArgs args) {
        if (OnLeaningEnd != null) OnLeaningEnd(this, args);
    }
    protected void Reload(object sender, EventArgs e) {
        Ammo ammo = gun.GetComponentInChildren<Ammo>();
        if(ammo == null || isReloading) return;
        if (OnReload != null) OnReload(this, EventArgs.Empty);
        isReloading = true;
        Vector3 originalPosition = ammo.transform.localPosition;
        Quaternion originalRotation = ammo.transform.localRotation;

        GameObject newAmmo = null;

        StartCoroutine(ExecuteAfterTime(0.3f, () => {
            ammo.transform.parent = gun.ammoPosition.parent;
            ammo.transform.position = gun.ammoPosition.position;
            ammo.transform.rotation = gun.ammoPosition.rotation;
        }));
        StartCoroutine(ExecuteAfterTime(0.7f, () => {
            ammo.transform.parent = null;
            Vector3 throwDirection = new Vector3(1, -1, 0);
            float throwForce = 1.5f;

            Rigidbody rb = ammo.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
            ammo.GetComponent<BoxCollider>().enabled = true;
            //ammo.GetComponent<Rigidbody>().isKinematic = true;
        }));
        StartCoroutine(ExecuteAfterTime(0.8f, () => {
            newAmmo = Instantiate(gun.ammoPrefab, gun.ammoPosition.transform.position, gun.ammoPosition.rotation);
            newAmmo.transform.parent = gun.ammoPosition.parent;
            newAmmo.transform.position = gun.ammoPosition.position;
            newAmmo.transform.rotation = gun.ammoPosition.rotation;
        }));
        StartCoroutine(ExecuteAfterTime(1.7f, () => {
            newAmmo.transform.parent = gun.transform;
            newAmmo.transform.localPosition = originalPosition;
            newAmmo.transform.localRotation = originalRotation;
            gun.currentAmmo = gun.maxAmmo;
            isReloading = false;
        }));
    }

    protected void ChangeWeapon(object sender, InputValueEventArgs args) {
        WeaponTypes weapon = (WeaponTypes)args.Value;

        if(gun != null) {
            gun.transform.position = gun.positionOnBody.position;
            gun.transform.rotation = gun.positionOnBody.rotation;
            gun.transform.parent = gun.positionOnBody.parent;
        }

        currentWeapon = weapon;
        MultiAimConstraint rigConstraints = GetComponentInChildren<MultiAimConstraint>();

        switch (weapon) {
            case (WeaponTypes.Pistol):
                gun = GetComponentInChildren<Pistol>();
                gun.transform.position = gun.equippedPositions[0].transform.position;
                gun.transform.rotation = gun.equippedPositions[0].transform.rotation;
                gun.transform.parent = gun.equippedPositions[0].transform.parent;
                rigConstraints.data.offset = new Vector3(20, 17, 13);


                break;
            case (WeaponTypes.AssultRifle):
                gun = GetComponentInChildren<AssultRifle>();
                int index = 0;
                if (isAiming) index = 1;
                gun.transform.position = gun.equippedPositions[index].transform.position;
                gun.transform.rotation = gun.equippedPositions[index].transform.rotation;
                gun.transform.parent = gun.equippedPositions[index].transform.parent;
                rigConstraints.data.offset = new Vector3(15, 25, 0);
                break;
        }

        if (OnWeaponChange != null) OnWeaponChange(this, args);
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
    public void ApplyDamage(BulletData bulletData) {
        this.health -= bulletData.damage;
        if (OnHealthLoss != null) OnHealthLoss(this, new BulletDataEventArgs(bulletData));

        if (this.health <= 0) {
            if (OnDeath != null) OnDeath(this, EventArgs.Empty);
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
        var moveAngle = Vector2.Angle(inputVector, new Vector2(aimingTarget.GetPosition().x - transform.position.x, aimingTarget.GetPosition().z - transform.position.z));

        float radians = (Mathf.PI / 180) * moveAngle;
        float x = 1f * Mathf.Sin(radians);
        float y = 1f * Mathf.Cos(radians);

        var moveDirection = Vector2.Angle(new Vector2(transform.right.x, transform.right.z), inputVector);


        if (moveDirection <= 90) {
            // Right
        } else {
            // Left
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
    public bool IsCrouching() {
        return isCrouching;
    }

}
