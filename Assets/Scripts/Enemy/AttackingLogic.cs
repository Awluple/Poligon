using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Poligon.Ai.EnemyStates;
using static UnityEditor.PlayerSettings;

public class AttackingLogic : MonoBehaviour
{

    private EnemyController enemyController;
    private IEnumerator movingAttackCoroutine;
    private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator shootingCoroutine;
    private IEnumerator keepTrackOnEnemyCoroutine;




    private float lastSeen;
    private Vector3 lastKnownPosition = Vector3.zero;
    private bool alerted = false;
    private bool aiming = false;

    public CoverPosition coverPosition;

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }

    public void SetBehindCoverPosition() {
        if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);

        if(!HasVisionOnOpponent(out Character chara)) {
            enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
            List<CoverPose> poses = coverPosition.GetCoverPoses();
            if (poses.Count == 1) {

                if (poses[0] == CoverPose.Standing) {
                    enemyController.CrouchStart();
                }
            }
        }

        coveredAttackCoroutine = ContinueAttackingWhileCovered();
        StartCoroutine(coveredAttackCoroutine);
    }

    private void StopAttacking(object sender = null, System.EventArgs e = null) {
        if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);
        if (movingAttackCoroutine != null) StopCoroutine(checkLastSeen);
        alerted = false;
        enemyController.AimCancel();
        enemyController.aiState = AiState.Patrolling;
        enemyController.currentPatrolPosition = -1;
        enemyController.SetPatrollingPath();
    }

    public void EnemySpotted() {
        Player player = FindFirstObjectByType<Player>();
        Vector3 hidingSpot = enemyController.hidingLogic.GetHidingPosition(player.transform.position);

        coverPosition = enemyController.hidingLogic.currentCoverPosition;
        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        checkLastSeen = CheckLastSeen();
        StartCoroutine(checkLastSeen);
        enemyController.aiState = AiState.Hiding;

        keepTrackOnEnemyCoroutine = KeepTrackOnEnemy();
        StartCoroutine(keepTrackOnEnemyCoroutine);


        if (hidingSpot != Vector3.zero) {
            //Vector3 towards = Vector3.RotateTowards(transform.forward, player.transform.position- transform.position, 999f, 999f);
            //Debug.DrawRay(transform.position, towards * 10, Color.red, 40f);
            //Debug.Log(Vector3.Angle(towards, hidingSpot - transform.position));
            enemyController.SetNewDestinaction(hidingSpot);
            enemyController.OnFinalPositionEvent += enemyController.SetAiState;
            enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

        } else {
            enemyController.SetNewDestinaction(transform.position);
            enemyController.CrouchStart();
            //enemyController.SetAiState();
        }
        

        //AimStart();
        if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);
        movingAttackCoroutine = ContinueAttackingWhileMoving();
        StartCoroutine(movingAttackCoroutine);
    }

    IEnumerator CheckLastSeen() {
        for (; ; ) {
            if (Time.time - lastSeen > 50f && enemyController.aiState != AiState.Chasing) {
                enemyController.aiState = AiState.Chasing;
                enemyController.SetNewDestinaction(lastKnownPosition);
                if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);
                enemyController.RunCancel();
                enemyController.enemy.GetAimPosition().Reset();
            } else if (Time.time - lastSeen > 60f) {
                StopAttacking();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ContinueAttackingWhileMoving(bool runWhenNoVision = true) {
        if (shootingCoroutine == null) {
            shootingCoroutine = ShootingCoroutine();
            StartCoroutine(shootingCoroutine);
        }
        for (; ; ) {
            if(HasVisionOnOpponent(out Character character)) {
                if (!aiming) enemyController.AimStart();
                if (character == enemyController.enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...
            } else {
                if(runWhenNoVision) {
                    enemyController.AimCancel();
                    enemyController.RunStart();
                }
            }
            yield return new WaitForSeconds(Random.Range(0.8f, 1.2f));
        }
    }

    bool HasVisionOnOpponent(out Character character) {

        Vector3 eyesPosition = enemyController.eyes.transform.position;
        Ray ray = new Ray(eyesPosition, enemyController.enemy.GetAimPosition().transform.position - eyesPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 40f)) {
            if (hit.collider.gameObject.TryGetComponent<Character>(out Character hitCharacter)) {
                character = hitCharacter;
                return true;
            }

        }
        character = null;
        return false;
    }

    IEnumerator ShootingCoroutine() {
        for (; ; ) {
            if(HasVisionOnOpponent(out Character character)) {
                if (character != enemyController.enemy) enemyController.ShootPerformed();
            } else {

                //enemyController.enemy.GetAimPosition().MoveAim(lastKnownPosition, 90f, null);
            }
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        }
        
    }

    IEnumerator KeepTrackOnEnemy() {
        bool needsReposition = false;
        for (; ; ) {
            Character character = null;
            bool hasVis = HasVisionOnOpponent(out character);
            if (hasVis) {
                lastKnownPosition = character.transform.position;
                lastSeen = Time.time;
                needsReposition = true;
            } else {
                if(needsReposition) enemyController.enemy.GetAimPosition().Reposition(lastKnownPosition);
                needsReposition = false;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator ContinueAttackingWhileCovered() {
        bool covered = true;
        int lastSeenIteration = 0;

        if (shootingCoroutine == null) {
            shootingCoroutine = ShootingCoroutine();
            StartCoroutine(shootingCoroutine);
        }
        for (; ; ) {
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            if(HasVisionOnOpponent(out Character character) ) {
                lastSeenIteration = 0;
                goto End;
            }
            lastSeenIteration++;
            if (lastSeenIteration == 5) {
                Vector3 hidingSpot = enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position, enemyController.enemy.GetAimPosition().transform.position, true, true, 3f, 30f, 11f);
                coverPosition = enemyController.hidingLogic.currentCoverPosition;
                if (hidingSpot != Vector3.zero) {
                    enemyController.enemy.GetAimPosition().Reposition(lastKnownPosition);
                    enemyController.AimStart();
                    enemyController.CrouchCancel();

                    if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);
                    movingAttackCoroutine = ContinueAttackingWhileMoving(false);
                    StartCoroutine(movingAttackCoroutine);

                    enemyController.SetNewDestinaction(hidingSpot);
                    StopCoroutine(coveredAttackCoroutine);
                    lastSeen = 30f;
                }
                goto End;
            }


            if (covered) {
                enemyController.CrouchCancel();
                enemyController.AimStart();
                enemyController.aiState = AiState.Attacking;
                Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;

                enemyController.enemy.GetAimPosition().Reposition(lastKnownPosition == Vector3.zero ? pos : lastKnownPosition);
                enemyController.enemy.RotateSelf(coverPosition.transform.forward);
                if(shootingCoroutine == null) {
                    shootingCoroutine = ShootingCoroutine();
                    StartCoroutine(shootingCoroutine);
                }
                covered = false;
            } else {
                enemyController.CrouchStart();
                enemyController.AimCancel();
                enemyController.aiState = AiState.BehindCover;
                enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
                covered = true;
                
                if (shootingCoroutine != null) {
                    StopCoroutine(shootingCoroutine);
                    shootingCoroutine = null;
                }
            }
            End:;

        }
    }
}
