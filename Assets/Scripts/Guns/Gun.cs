using Poligon.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BulletData {
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

public abstract class Gun : MonoBehaviour
{

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] private AimPosition aimingTarget;
    [SerializeField] private float fireRate;
    [SerializeField] private AudioClip shotSound;


    private float nextShootTime;
    private AudioSource audioSource;


    void Awake()
    {
        GameObject shotSoundObject = new GameObject("ShotSoundObject");
        audioSource = shotSoundObject.AddComponent<AudioSource>();
        audioSource.volume = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot() {

        if(CanShoot()) {
            Vector3 target = aimingTarget.GetPosition();
            bullet.Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(target, Vector3.up), new BulletData(target - spawnBulletPosition.transform.position, GetComponentInParent<Character>(), 50));
            nextShootTime = Time.time + fireRate;
            audioSource.PlayOneShot(shotSound);
        }
    }

    public bool CanShoot() {
        return Time.time > nextShootTime;
    }
}
