using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAimPosition : AimPosition {
    Player player;
    Enemy enemy;
    public bool aimingAtPlayer = false;

    public event EventHandler OnLineOfSight;
    public event EventHandler OnLineOfSightLost;

    public bool sightEventsCalled = false;

    private IEnumerator checkCoroutine;

    bool justNoticed = true;
    Vector3 movePosition = Vector3.zero;
    float moveSpeed = 2f;
    public delegate void OnMoveCompleteCallback();
    OnMoveCompleteCallback onMoveCompleteCallback;


    void Awake() {
        enemy = GetComponentInParent<Enemy>();
        enemy.OnDeath += RemoveTarget;
        player = FindFirstObjectByType<Player>().GetComponent<Player>();
        checkCoroutine = DoCheck(true);
        StartCoroutine(checkCoroutine);
        transform.SetParent(null);
    }

    // Update is called once per frame
    void Update() {
        if(movePosition != Vector3.zero) {
            Debug.Log("Moving?");
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, movePosition, step);
        }
        if (Vector3.Distance(transform.position, movePosition) < 0.05f) {
            transform.position = movePosition;
            StopMoveAim();
        }
    }

    void RemoveTarget(object sender, EventArgs args) {
        Destroy(gameObject);
    }

    IEnumerator DoCheck(bool repositionOnFailed) {
        bool moveAimCalled = false;
        for (; ; ) {
            if (ProximityCheck()) {
                Vector3 position = player.transform.position;
                
                if (player.IsCrouching()) {
                    position.y += 1.2f;
                } else {
                    position.y += 1.8f;
                }
                aimingAtPlayer = true;
                if (justNoticed) {
                    if(!moveAimCalled) {
                        Vector3 initialPosition = enemy.transform.forward * 4f + enemy.transform.position;
                        if(enemy.IsCrouching()) {
                            initialPosition.y += 1.2f;
                        } else {
                            initialPosition.y += 1.8f;
                        }
                        transform.position = initialPosition;
                        MoveAim(position, 80f, () => { Debug.Log("Callback?"); justNoticed = false; });
                        moveAimCalled = true;
                    }
                    
                } else {
                    transform.position = position;
                }
                yield return new WaitForSeconds(0f);
            } else {
                if (repositionOnFailed) {
                    transform.position = enemy.transform.position;
                }
                justNoticed = true;
                aimingAtPlayer = false;
                yield return new WaitForSeconds(.4f);
            }
        }
    }

    public void Reset() {
        transform.position = enemy.transform.position;
        aimingAtPlayer = false;
        sightEventsCalled = false;
    }

    public override void Reposition(Vector3 newPosition) {
        base.Reposition(newPosition);
        StopCoroutine(checkCoroutine);
        aimingAtPlayer = false;
        sightEventsCalled = false;
        checkCoroutine = DoCheck(false);
        StartCoroutine(checkCoroutine);
    }

    public void MoveAim(Vector3 targetPosition, float speed, OnMoveCompleteCallback onCompleteCallback) {
        onMoveCompleteCallback = onCompleteCallback;
        movePosition = targetPosition;
        moveSpeed = speed;
    }
    public void StopMoveAim() {
        movePosition = Vector3.zero;
        moveSpeed = 2f;
        if(onMoveCompleteCallback != null) {
            onMoveCompleteCallback();
        }
    }

    bool ProximityCheck() {
        //if (aimingAtPlayer && Vector3.Distance(enemy.transform.position, player.transform.position) < 40f) { // keep track of the target if detected
        //    return true;
        //}
        if (aimingAtPlayer) { // keep track of the target if detected
            return true;
        }


        if (Vector3.Distance(enemy.transform.position, player.transform.position) < 5f) { // Always detect within 5f distance
            if (OnLineOfSight != null && sightEventsCalled == false) {
                OnLineOfSight(this, EventArgs.Empty);
                sightEventsCalled = true;
            };
            return true;
        } else if (Vector3.Distance(enemy.transform.position, player.transform.position) < 20f &&
            Vector3.Angle(player.transform.position - enemy.transform.position, enemy.transform.forward) < 75) { // Detect within 20f if visable
            Vector3 startPoint = enemy.GetController().eyes.transform.position;
            startPoint.y += 1f;
            Vector3 target = player.transform.position;
            target.y += 1.6f;
            Ray ray = new Ray(startPoint, target - startPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, 25f)) {
                if (!hit.collider.gameObject.TryGetComponent<Player>(out Player player)) {
                    return false;
                }

            }

            if (OnLineOfSight != null && sightEventsCalled == false) {
                OnLineOfSight(this, EventArgs.Empty);
                sightEventsCalled = true;
            };
            return true;
        }
        if (aimingAtPlayer) {
            if (OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
            sightEventsCalled = false;
        }
        return false;
    }
}
