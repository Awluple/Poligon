using Poligon.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            bullet.Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(target, Vector3.up), target - spawnBulletPosition.transform.position);
            nextShootTime = Time.time + fireRate;
            audioSource.PlayOneShot(shotSound);
        }
    }

    public bool CanShoot() {
        return Time.time > nextShootTime;
    }
}
