using Poligon.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds information about a bullet
/// </summary>
public struct BulletData {
    /// <param name="targetPos">Position of the target</param>
    /// <param name="src">The character that shot the bullet</param>
    /// <param name="dmg">Damage to apply on hit</param>
    public BulletData(Vector3 targetPos, Character src, float dmg) {
        targetPosition = targetPos;
        source = src;
        damage = dmg;
    }

    public Vector3 targetPosition;
    public Character source;
    public float damage;
}
public class BulletDataEventArgs : EventArgs {
    public BulletDataEventArgs(BulletData bulletData) {
        BulletData = bulletData;
    }

    public BulletData BulletData { get; set; }
}
/// <summary>
/// Holds all information about a gun, such as ammunition, sounds, positions on body and accurancy.
/// Creates bullets when shooting and validates shooting (firerate).
/// </summary>
public abstract class Gun : MonoBehaviour
{

    [SerializeField] protected GameObject bullet;
    [SerializeField] protected Transform spawnBulletPosition;
    [SerializeField] protected AimPosition aimingTarget;
    // Ammo
    public GameObject ammoPrefab;
    public float fireRate;
    public int maxAmmo;
    public int currentAmmo;
    public int ammoStock;
    // Shooting Accurancy
    public float baseInaccuracy = 0.1f; // Base inaccuracy factor
    public float inaccuracyIncreaseRate = 0.01f; // Rate at which inaccuracy increases on each shot
    public float inaccuracyDecreaseRate = 0.005f; // Rate at which inaccuracy decreases over time
    public float maxInaccuracy = 0.3f; // Maximum inaccuracy
    private float currentInaccuracy = 0f;
    public float timeToResetAccurancy = 0f;
    private float timeToResetAccurancyTimeout = 0f;
    // Sounds
    [SerializeField] protected AudioClip shotSound;
    [SerializeField] protected float shotSoundVolume = 0.3f;
    [SerializeField] protected AudioClip reloadSound;
    [SerializeField] protected float reloadSoundVolume = 0.3f;
    public bool automatic;
    //Positions of a gun
    public Transform positionOnBody;
    public Transform[] equippedPositions;
    public Transform ammoPosition;

    private float nextShootTime; // When a gun can shoot again; for non-automatic
    private AudioSource audioSource;


    protected void Awake()
    {
        GameObject shotSoundObject = new GameObject("ShotSoundObject");
        audioSource = shotSoundObject.AddComponent<AudioSource>();
        currentInaccuracy = baseInaccuracy;
    }

    protected void Update()
    {
        if (timeToResetAccurancyTimeout < 0 && currentInaccuracy > baseInaccuracy) {
            float rate = currentInaccuracy > maxInaccuracy / 4 ?
                1 + 2 * currentInaccuracy :
                1; // if innacurancy > 25%, increase innacurancy decrease speed (more innacure - faster)
            currentInaccuracy -= inaccuracyDecreaseRate * Time.deltaTime * rate;
        } else if(timeToResetAccurancyTimeout > 0) {
            timeToResetAccurancyTimeout -= Time.deltaTime;
        }
    }
    /// <summary>
    /// Fire a bullet, checks if a gun is able to shoot
    /// </summary>
    public void Shoot() {
        if (!CanShoot()) return;
        //Appy accurancy to the shot
        Vector3 target = aimingTarget.GetPosition();
        Vector3 direction = target - spawnBulletPosition.transform.position;
        direction.x += UnityEngine.Random.Range(-currentInaccuracy, currentInaccuracy);
        direction.y += UnityEngine.Random.Range(-currentInaccuracy, currentInaccuracy);
        direction.z += UnityEngine.Random.Range(-currentInaccuracy, currentInaccuracy);
        if(currentInaccuracy < maxInaccuracy) {
            currentInaccuracy += inaccuracyIncreaseRate;
        }

        bullet.Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(target, Vector3.up), new BulletData(direction, GetComponentInParent<Character>(), 50));
        nextShootTime = Time.time + fireRate;
        audioSource.volume = shotSoundVolume;
        audioSource.PlayOneShot(shotSound);

        timeToResetAccurancyTimeout = timeToResetAccurancy;
    }

    public bool CanShoot() {
        return Time.time > nextShootTime;
    }
    public void PlayReloadSound() {
        audioSource.volume = reloadSoundVolume;
        audioSource.PlayOneShot(reloadSound);
    }
    /// <summary>
    /// Switches the position of the gun
    /// </summary>
    /// <param name="position">Index of a position in equippedPositions</param>
    public void SwitchPosition(int position) {
        transform.position = equippedPositions[position].transform.position;
        transform.rotation = equippedPositions[position].transform.rotation;
        transform.parent = equippedPositions[position].transform.parent;
    }
    /// <summary>
    /// Switches the position of the gun back to the unequipped position
    /// </summary>
    public void UnequipWeapon() {
        transform.position = positionOnBody.position;
        transform.rotation = positionOnBody.rotation;
        transform.parent = positionOnBody.parent;
    }
}
