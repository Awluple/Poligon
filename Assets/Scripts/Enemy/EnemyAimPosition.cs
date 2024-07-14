using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class EnemyAimPosition : AimPosition {
    [SerializeField] CharactersSphere charactersSphere;
    [SerializeField] Enemy enemy;
    public bool aimingAtCharacter = false;
    public bool alerted = false;

    public event EventHandler OnLineOfSight;
    public event EventHandler OnLineOfSightLost;

    public bool sightEventsCalled = false;

    private IEnumerator checkCoroutine;

    Character opponent;
    DetectionPoint opponentDetectionPoint;
    bool justNoticed = true;
    Vector3 movePosition = Vector3.zero;
    float moveSpeed = 2f;
    public delegate void OnMoveCompleteCallback();
    OnMoveCompleteCallback onMoveCompleteCallback;


    void Awake() {
        enemy = GetComponentInParent<Enemy>();
        enemy.OnDeath += RemoveTarget;
        checkCoroutine = DoCheck(true);
        StartCoroutine(checkCoroutine);
        transform.SetParent(enemy.transform.parent);
    }

    // Update is called once per frame
    void Update() {
        if (movePosition != Vector3.zero) {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, movePosition, step);
        }
        if (Vector3.Distance(transform.position, movePosition) < 0.05f) {
            transform.position = movePosition;
            StopMoveAim();
        }
    }
    /// <summary>
    /// Returns the current position of the target, if currenlty moving by MoveAim method, returns final movement position instead.
    /// </summary>
    /// <returns>transform.position or move position if during MoveAim</returns>
    public override Vector3 GetPosition() {
        if (movePosition == Vector3.zero) {
            return transform.position;
        } else {
            return movePosition;
        }
    }

    void RemoveTarget(object sender, EventArgs args) {
        Destroy(gameObject);
    }
    /// <summary>
    /// Moves the target to a character
    /// </summary>
    /// <param name="character">The character to move the aim</param>
    /// <param name="moveAimCalled">If true the MoveAim has been called, to prevent unnecessary calls</param>
    void MoveToCharacter(Character character, ref bool moveAimCalled, DetectionPoint detectionPoint = null) {
        if (detectionPoint == null) {
            if (character.detectionPoints.Length == 0) {
                Debug.LogError($"The {character} object does not have a DetectionPoint");
                return;
            }
            detectionPoint = character.detectionPoints[0];
        }
        Vector3 position = detectionPoint.transform.position;

        aimingAtCharacter = true;
        if (justNoticed) {
            if (!moveAimCalled) {
                Vector3 initialPosition = enemy.transform.forward * 4f + enemy.transform.position;
                if (enemy.IsCrouching()) {
                    initialPosition.y += 1.2f;
                } else {
                    initialPosition.y += 1.8f;
                }
                transform.position = initialPosition;
                MoveAim(position, 80f, () => { justNoticed = false; });
                moveAimCalled = true;
            }

        } else {
            transform.position = position;
        }
    }
    /// <summary>
    /// Performs a check if an enemy character is within range.
    /// </summary>
    /// <param name="repositionOnFailed">If true the target position goes back to the parent</param>
    IEnumerator DoCheck(bool repositionOnFailed) {
        bool moveAimCalled = false;
        for (; ; ) {
            if (charactersSphere.GetCharacters().Count == 0) {
                yield return new WaitForSeconds(.4f);
                continue;
            }
            if (opponentDetectionPoint != null) {
                Vector3 rayTarget = opponentDetectionPoint.transform.position;
                if (ProximityCheck(rayTarget)) {
                    MoveToCharacter(opponent, ref moveAimCalled, opponentDetectionPoint);
                    if (OnLineOfSight != null && sightEventsCalled == false) {
                        OnLineOfSight(this, EventArgs.Empty);
                        sightEventsCalled = true;
                    };
                    alerted = true;
                    yield return new WaitForSeconds(0f);
                } else {
                    opponentDetectionPoint = null;
                    opponent = null;
                }
            } else {
                foreach (var character in charactersSphere.GetCharacters().Values.ToList()) {
                    foreach (var detectionPoint in character.detectionPoints) {
                        Vector3 rayTarget = detectionPoint.transform.position;
                        if (ProximityCheck(rayTarget)) {
                            opponentDetectionPoint = detectionPoint;
                            opponent = character;
                        } else {
                            if (repositionOnFailed) {
                                transform.position = enemy.transform.position;
                            }
                            justNoticed = true;
                            aimingAtCharacter = false;
                            yield return new WaitForSeconds(.4f);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Resets the aim point back to the initial position
    /// </summary>
    public void Reset() {
        transform.position = enemy.transform.position;
        aimingAtCharacter = false;
        sightEventsCalled = false;
    }
    /// <summary>
    /// Immediately repositions the aim
    /// </summary>
    /// <param name="newPosition">The position</param>
    public override void Reposition(Vector3 newPosition) {
        base.Reposition(newPosition);
        StopMoveAim();
        StopCoroutine(checkCoroutine);
        aimingAtCharacter = false;
        sightEventsCalled = false;
        checkCoroutine = DoCheck(false);
        StartCoroutine(checkCoroutine);
    }
    /// <summary>
    /// Moves aim to the targetPosition
    /// </summary>
    /// <param name="targetPosition">Final position</param>
    /// <param name="speed">Speed of movement</param>
    /// <param name="onCompleteCallback">Called when on position</param>
    public void MoveAim(Vector3 targetPosition, float speed, OnMoveCompleteCallback onCompleteCallback = null) {
        onMoveCompleteCallback = onCompleteCallback;
        movePosition = targetPosition;
        moveSpeed = speed;
    }
    public void StopMoveAim() {
        movePosition = Vector3.zero;
        moveSpeed = 2f;
        if (onMoveCompleteCallback != null) {
            onMoveCompleteCallback();
        }
    }
    /// <summary>
    /// Move aim to character, skipping the proximity check
    /// </summary>
    /// <param name="character">The character to move the aim</param>
    public void LockOnTarget(Character character) {
        bool moveAimCalled = false;
        if (checkCoroutine != null) {
            StopCoroutine(checkCoroutine);
        }
        MoveToCharacter(character, ref moveAimCalled, null);
        if (OnLineOfSight != null) {
            OnLineOfSight(this, EventArgs.Empty);
            sightEventsCalled = true;
        };
        StartCoroutine(checkCoroutine);
    }

    /// <summary>
    /// Logic for checking if an enemy is within range
    /// </summary>
    /// <returns>True if within range</returns>
    bool ProximityCheck(Vector3 rayTarget) {
        if (aimingAtCharacter) { // keep track of the target if detected
            return true;
        }
        int layerMask = (1 << 8) | (1 << 9); // ingore Enemy and Character masks
        layerMask = ~layerMask;
        Vector3 rayStartPoint = enemy.GetController().eyes.transform.position;
        Ray ray = new Ray(rayStartPoint, rayTarget - rayStartPoint);
        float distanceToEnemy = Vector3.Distance(enemy.transform.position, rayTarget);

        if (distanceToEnemy < 4f &&
            Physics.Raycast(ray, out RaycastHit hit2, (alerted ? 40f : 25f), layerMask)) { // Always detect within 4f distance
            if (OnLineOfSight != null && sightEventsCalled == false) {
                OnLineOfSight(this, EventArgs.Empty);
                sightEventsCalled = true;
            };
            return true;
        } else if (distanceToEnemy < (alerted ? 35f : 20f) && // Detect within 20f if visable or 35f if alerted
            Vector3.Angle(rayTarget - enemy.transform.position, enemy.transform.forward) < 75) { // Must be within 75 degerees angle
            if (Physics.Raycast(ray, out RaycastHit hit, (alerted ? 40f : 25f), layerMask)) {
                if (!hit.collider.gameObject.TryGetComponent<Character>(out Character player)) {
                    return false;
                }

            }
            return true;
        }
        if (aimingAtCharacter) {
            if (OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
            sightEventsCalled = false;
        }
        return false;
    }
}
