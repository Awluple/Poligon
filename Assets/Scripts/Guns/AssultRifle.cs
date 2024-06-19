using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : Gun {
    // Start is called before the first frame update
    void Start() {
        positionOnBody = FindFirstObjectByType<AssultRiflePosition>().transform;
    }

    // Update is called once per frame
    void Update() {
        base.Update();
    }
    override public void Reload(Ammo ammo = null, Action callback = null) {
        if (ammo == null) {
            ammo = GetComponentInChildren<Ammo>();
        }
        isReloading = true;
        Vector3 originalPosition = ammo.transform.localPosition;
        Quaternion originalRotation = ammo.transform.localRotation;

        GameObject newAmmo = null;

        StartCoroutine(ExecuteAfterTime(0.3f, () => {
            ammo.transform.parent = ammoPosition.parent;
            ammo.transform.position = ammoPosition.position;
            ammo.transform.rotation = ammoPosition.rotation;
            PlayReloadSound();
        }));
        StartCoroutine(ExecuteAfterTime(0.7f, () => {
            ammo.transform.parent = null;
            Vector3 throwDirection = new Vector3(UnityEngine.Random.Range(-1.2f, -0.8f), UnityEngine.Random.Range(0.8f, 1.2f), 0);
            float throwForce = 1.5f;

            Rigidbody rb = ammo.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(transform.rotation * throwDirection.normalized * throwForce, ForceMode.Impulse);
            ammo.GetComponent<BoxCollider>().enabled = true;
        }));
        StartCoroutine(ExecuteAfterTime(0.8f, () => {
            newAmmo = Instantiate(ammoPrefab, ammoPosition.transform.position, ammoPosition.rotation);
            newAmmo.transform.parent = ammoPosition.parent;
            newAmmo.transform.position = ammoPosition.position;
            newAmmo.transform.rotation = ammoPosition.rotation;
        }));
        StartCoroutine(ExecuteAfterTime(1.7f, () => {
            newAmmo.transform.parent = transform;
            newAmmo.transform.localPosition = originalPosition;
            newAmmo.transform.localRotation = originalRotation;

            if (ammoStock >= maxAmmo) {
                currentAmmo = maxAmmo;
                ammoStock -= maxAmmo;
            } else {
                currentAmmo = ammoStock;
                ammoStock = 0;
            }
        }));
        StartCoroutine(ExecuteAfterTime(1.8f, () => {
            isReloading = false;
            if(callback != null) {
                callback();
            }
        }));
    }
}
