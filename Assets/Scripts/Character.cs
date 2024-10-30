using Poligon.Enums;
using Poligon.EvetArgs;
using System;
using System.Collections;
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

    public Team team;


    [SerializeField] protected Rig rig;
    public Gun gun = null;
    public WeaponTypes currentWeapon = WeaponTypes.None;
    [SerializeField] protected AimPosition aimingTarget;

    protected Vector3 movement = Vector3.zero;

    protected CharacterController characterController;
    [SerializeField] protected bool isWalking;
    [SerializeField] protected bool isRunning;
    [SerializeField] protected bool isAiming;
    [SerializeField] protected bool isCrouching;
    [SerializeField] protected bool stunned = false;

    private bool lastFrameMoving = false;


    [SerializeField] protected float _health = 100;
    [SerializeField] protected float _maxHealth = 100;
    public float health {
        get { return _health; }
        private set { _health = value; }
    }
    public float maxHealth {
        get { return _maxHealth; }
        private set { _maxHealth = value; }
    }

    public DetectionPoint[] detectionPoints;

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
    public event EventHandler OnReloadEnd;

    public event EventHandler<InputValueEventArgs> OnLeaning;
    public event EventHandler<InputValueEventArgs> OnLeaningEnd;
    public event EventHandler<InputValueEventArgs> OnWeaponChange;

    public event EventHandler<CharacterEventArgs> OnDeath;
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
    protected virtual void Awake() {
        maxHealth = health;
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
        if (currentWeapon == WeaponTypes.None) {
            return;
        }
        if (isGrounded() && !stunned && health > 0) {

            if (OnAiming != null) OnAiming(this, EventArgs.Empty);
            isAiming = true;
            isRunning = false;
            rig.weight = 1;
        }
        switch (currentWeapon) {
            case (WeaponTypes.Pistol):

                break;
            case (WeaponTypes.AssultRifle):
                gun.SwitchPosition(1);
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
                gun.SwitchPosition(0);
                break;
        }
    }

    protected void ShootPerformed(object sender, System.EventArgs e) {
        if (!isAiming || gun.isReloading) return;
        if (gun.currentAmmo == 0) return;
        if (!gun.automatic) {
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
        if (OnShootEnd != null) OnShootEnd(this, EventArgs.Empty);
        if (shootingCoroutine != null) {
            StopCoroutine(shootingCoroutine);
        }
    }

    protected void LeaningStart(object sender, InputValueEventArgs args) {
        if (OnLeaning != null) OnLeaning(this, args);
    }

    protected void LeaningCancel(object sender, InputValueEventArgs args) {
        if (OnLeaningEnd != null) OnLeaningEnd(this, args);
    }
    /// <summary>
    /// Reload a weapon. Move ammo object to the hand, drop it, and spawn a new one.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Reload(object sender, EventArgs e) {
        Ammo ammo = gun.GetComponentInChildren<Ammo>();
        if (ammo == null || gun.isReloading || gun.ammoStock <= 0) return;
        if (OnShootEnd != null) OnShootEnd(this, EventArgs.Empty);
        if (shootingCoroutine != null) {
            StopCoroutine(shootingCoroutine);
        }
        if (OnReload != null) OnReload(this, EventArgs.Empty);
        gun.Reload(null, () => {
            ReloadEnd();
        });
    }

    protected void ReloadEnd() {
        if (OnReloadEnd != null) OnReloadEnd(this, EventArgs.Empty);
    }

    protected void ChangeWeapon(object sender, InputValueEventArgs args) {
        WeaponTypes weapon = (WeaponTypes)args.Value;

        if (gun != null) {
            gun.UnequipWeapon();
        }

        currentWeapon = weapon;
        MultiAimConstraint rigConstraints = GetComponentInChildren<MultiAimConstraint>();

        switch (weapon) {
            case (WeaponTypes.Pistol):
                gun = GetComponentInChildren<Pistol>();
                gun.SwitchPosition(0);
                rigConstraints.data.offset = new Vector3(20, 17, 13);

                break;
            case (WeaponTypes.AssultRifle):
                gun = GetComponentInChildren<AssultRifle>();
                int index = 0;
                if (isAiming) index = 1;
                gun.SwitchPosition(index);
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
        if (bulletData.source.team == team) return;
        health -= bulletData.damage;
        if (OnHealthLoss != null) OnHealthLoss(this, new BulletDataEventArgs(bulletData));

        if (health <= 0) {
            if (OnDeath != null) OnDeath(this, new CharacterEventArgs(this));
            OnDeath = null;
            Destroy(rig);
            rig = null;
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
        if (aimingTarget.GetPosition() == null) Debug.Log("ASDASD");
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
