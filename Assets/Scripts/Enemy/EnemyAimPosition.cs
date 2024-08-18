using Poligon.EvetArgs;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyAimPosition : AimPosition {
    [SerializeField] CharactersSphere charactersSphere;
    [SerializeField] Enemy enemy;
    public bool aimingAtCharacter = false;
    public bool alerted = false;
    public float alertedMaxDistance = 40f;

    public event EventHandler<CharacterEventArgs> OnLineOfSight;
    public event EventHandler OnLineOfSightLost;

    private IEnumerator checkCoroutine;
    private IEnumerator lookForEnemiesCoroutine;

    Character opponent;
    DetectionPoint opponentDetectionPoint;
    Vector3 movePosition = Vector3.zero;
    float moveSpeed = 2f;
    public delegate void OnMoveCompleteCallback();
    OnMoveCompleteCallback onMoveCompleteCallback;


    void Awake() {
        enemy = GetComponentInParent<Enemy>();
        transform.SetParent(enemy.transform.parent);
    }
    private void Start() {
        enemy.OnDeath += RemoveTarget;
        checkCoroutine = DoCheck(false);
        StartCoroutine(checkCoroutine);

        lookForEnemiesCoroutine = LookForEnemies();
        StartCoroutine(lookForEnemiesCoroutine);
    }

    // Update is called once per frame
    void Update() {
        if (movePosition != Vector3.zero) {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, movePosition, step);
        }
        if (Vector3.Distance(transform.position, movePosition) < 0.05f) {
            ResetMoveAim();
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
        StopAllCoroutines();
        enemy.OnDeath -= RemoveTarget;
        StartCoroutine(ExecuteAfterTime(0f));
    }
    IEnumerator ExecuteAfterTime(float time) {
        yield return new WaitForSeconds(time);

        Destroy(gameObject);
    }
    /// <summary>
    /// Moves the target to a character
    /// </summary>
    /// <param name="character">The character to move the aim</param>
    /// <param name="moveAimCalled">If true the MoveAim has been called, to prevent unnecessary calls</param>
    /// <param name="immediately">When true the aim will teleport to the position, when false it will slowly move there</param>
    void MoveToCharacter(Character character, ref bool moveAimCalled, DetectionPoint detectionPoint = null, bool immediately = false) {
        if (detectionPoint == null) {
            if (character.detectionPoints.Length == 0) {
                Debug.LogError($"The {character} object does not have a DetectionPoint");
                return;
            }
            detectionPoint = character.detectionPoints[0];
        }
        Vector3 position = detectionPoint.transform.position;
        if (!aimingAtCharacter) {
            if (!moveAimCalled) {
                aimingAtCharacter = false;
                if (immediately) { 
                    transform.position = position;
                }
                else {
                    MoveAim(position, 80f, () => { aimingAtCharacter = true; });
                }
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
            if (charactersSphere.GetEnemyCharacters().Count == 0) {
                yield return new WaitForSeconds(.4f);
                continue;
            }
            if (opponentDetectionPoint != null) {
                Vector3 rayTarget = opponentDetectionPoint.transform.position;
                if (ProximityCheck(rayTarget)) {
                    MoveToCharacter(enemy.GetController().attackingLogic.opponent, ref moveAimCalled, opponentDetectionPoint);
                    if (OnLineOfSight != null) {
                        OnLineOfSight(this, new CharacterEventArgs(opponent));
                    };
                    alerted = true;
                } else {
                    moveAimCalled = false;
                    ResetMoveAim();
                    opponentDetectionPoint = null;
                    opponent = null;
                    if (aimingAtCharacter && OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
                    aimingAtCharacter = false;
                }
                yield return new WaitForSeconds(0f);
            } else {
                if(GetCharacter()) {
                    yield return new WaitForSeconds(0f);
                }else {
                    yield return new WaitForSeconds(.4f);
                }


                //bool found = false;
                //foreach (var character in charactersSphere.GetEnemyCharacters().Values.ToList()) {
                //    if (found) break;
                //    foreach (var detectionPoint in character.detectionPoints) {
                //        //if (detectionPoint == null) {
                //        //    opponent = null;
                //        //    opponentDetectionPoint = null;
                //        //    yield return new WaitForSeconds(.4f);
                //        //    break;
                //        //} else {
                //            Vector3 rayTarget = detectionPoint.transform.position;
                //            if (ProximityCheck(rayTarget)) {
                //                opponentDetectionPoint = detectionPoint;
                //                opponent = character;
                //                found = true;
                //                yield return new WaitForSeconds(0f);
                //                break;
                //            } else {
                //                if (repositionOnFailed) {
                //                    transform.position = enemy.transform.position;
                //                }
                //                yield return new WaitForSeconds(.4f);
                //            }

                //    }
                //}
                //yield return new WaitForSeconds(0f);
            }
        }
    }

    bool GetCharacter() {
        foreach (var character in charactersSphere.GetEnemyCharacters().Values.ToList()) {
            foreach (var detectionPoint in character.detectionPoints) {
                if(detectionPoint == null) continue;
                Vector3 rayTarget = detectionPoint.transform.position;
                if (ProximityCheck(rayTarget)) {
                    opponentDetectionPoint = detectionPoint;
                    opponent = character;
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator LookForEnemies() {
        for(; ; ) {
            if(charactersSphere.GetEnemyCharacters().Values.Count == 0) {
                yield return new WaitForSeconds(UnityEngine.Random.Range(.4f, .6f));
                continue;
            }
            foreach (var character in charactersSphere.GetEnemyCharacters().Values.ToList()) {
                foreach (var detectionPoint in character.detectionPoints) {
                    if (detectionPoint == null) continue;
                    Vector3 rayTarget = detectionPoint.transform.position;
                    if (ProximityCheck(rayTarget)) {
                        OnLineOfSight(this, new CharacterEventArgs(character));
                    }
                }
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(.4f, .6f));
        }
    }

    /// <summary>
    /// Resets the aim point back to the initial position
    /// </summary>
    public void Reset() {
        transform.position = enemy.transform.position;
        if (aimingAtCharacter && OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
        aimingAtCharacter = false;
    }
    /// <summary>
    /// Immediately repositions the aim
    /// </summary>
    /// <param name="newPosition">The position</param>
    public override void Reposition(Vector3 newPosition) {
        base.Reposition(newPosition);
        ResetMoveAim();
        if (checkCoroutine != null) {
            StopCoroutine(checkCoroutine);
        }
        if (aimingAtCharacter && OnLineOfSightLost != null) OnLineOfSightLost(this, EventArgs.Empty);
        aimingAtCharacter = false;
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
    public void ResetMoveAim() {
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
    /// <param name="immediately">When true the aim will teleport to the position, when false it will slowly move there</param>
    public void LockOnTarget(Character character, bool immediately = false) {
        bool moveAimCalled = false;
        if (checkCoroutine != null) {
            StopCoroutine(checkCoroutine);
        }
        MoveToCharacter(character, ref moveAimCalled, null, immediately);
        opponent = character;
        opponentDetectionPoint = character.detectionPoints[0];
        //if (OnLineOfSight != null) {
        //    OnLineOfSight(this, new CharacterEventArgs(character));
        //};
        checkCoroutine = DoCheck(false);
        StartCoroutine(checkCoroutine);
    }

    public void LockOnTarget2(Character character, bool immediately = false) {
        bool moveAimCalled = false;
        if (checkCoroutine != null) {
            StopCoroutine(checkCoroutine);
        }
        MoveToCharacter(character, ref moveAimCalled, null, immediately);
        opponent = character;
        opponentDetectionPoint = character.detectionPoints[0];
        //if (OnLineOfSight != null) {
        //    OnLineOfSight(this, new CharacterEventArgs(character));
        //};
        checkCoroutine = DoCheck(false);
        StartCoroutine(checkCoroutine);
    }

    /// <summary>
    /// Logic for checking if an enemy is within range
    /// </summary>
    /// <returns>True if within range</returns>
    bool ProximityCheck(Vector3 rayTarget) {
        if (enemy == null) { return false; }
        int layerMask = (1 << 9) | (1 << 20); // ignore Enemy, Cover and Character masks
        if (enemy.team == Poligon.Enums.Team.Enemy) {
            layerMask |= (1 << 8);
        } else {
            layerMask |= (1 << 11);
        }
        layerMask = ~layerMask;
        Vector3 rayStartPoint = enemy.GetController().eyes.transform.position;
        Ray ray = new Ray(rayStartPoint, rayTarget - rayStartPoint);
        float distanceToEnemy = Vector3.Distance(enemy.transform.position, rayTarget);

        if (distanceToEnemy < 4f &&
            Physics.Raycast(ray, out RaycastHit hit2, (alerted ? alertedMaxDistance : 25f), layerMask)) { // Always detect within 4f distance
            return true;
        } else if (distanceToEnemy < (alerted ? alertedMaxDistance : 20f) && // Detect within 20f if visable or alertedMaxDistance if alerted
            Vector3.Angle(rayTarget - enemy.transform.position, enemy.transform.forward) < 75) { // Must be within 75 degerees angle
            if (Physics.Raycast(ray, out RaycastHit hit, (alerted ? alertedMaxDistance : 25f), layerMask)) {
                bool isChar = hit.collider.gameObject.TryGetComponent<Character>(out Character character);
                if (!isChar) {
                    return false;
                } else {
                    return true;
                }

            }
            return true;
        }
        return false;
    }
}
