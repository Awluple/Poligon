using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    void RemoveTarget(object sender, EventArgs args) {
        Destroy(gameObject);
    }

    IEnumerator DoCheck(bool repositionOnFailed) {
            for (; ; ) {
                if (ProximityCheck()) {
                    Vector3 position = player.transform.position;
                    position.y += 1.6f;
                    aimingAtPlayer = true;
                    transform.position = position;
                    yield return new WaitForSeconds(0f);
                } else {
                    if(repositionOnFailed) {
                        transform.position = enemy.transform.position;
                    }
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
                if(!hit.collider.gameObject.TryGetComponent<Player>(out Player player)) {
                    return false;
                }
                
            }

            if (OnLineOfSight != null && sightEventsCalled == false) {
                OnLineOfSight(this, EventArgs.Empty);
                sightEventsCalled = true;
            };
            return true;
        }
        if(aimingAtPlayer) {
            if(OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
            sightEventsCalled = false;
        }
        return false;
    }
}
