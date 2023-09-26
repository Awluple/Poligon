using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingLogic : MonoBehaviour
{

    private EnemyController enemyController;
    private IEnumerator movingAttackCoroutine;
    private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator shootingCoroutine;



    private float lastSeen;
    private Vector3 lastKnownPosition;
    private bool alerted = false;
    private bool aiming = false;

    public CoverPosition coverPosition;

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }

    public void SetBehindCoverPosition() {
        if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);


        enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
        List<CoverPose> poses = coverPosition.GetCoverPoses();
        if (poses.Count == 1 ) {
            
            if (poses[0] == CoverPose.Standing) {
                enemyController.CrouchStart();
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
        enemyController.state = AiState.Patrolling;
        enemyController.currentPatrolPosition = -1;
        enemyController.SetPatrollingPath();
    }

    public void EnemySpotted() {
        Vector3 hidingSpot = enemyController.hidingLogic.GetHidingPosition();
        coverPosition = enemyController.hidingLogic.currentCoverPosition;
        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        checkLastSeen = CheckLastSeen();
        StartCoroutine(checkLastSeen);
        enemyController.state = AiState.Hiding;


        if (hidingSpot != Vector3.zero) {
            //AimStart();
            enemyController.SetNewDestinaction(hidingSpot);
        } else {
            enemyController.SetNewDestinaction(transform.position);
        }



        enemyController.OnFinalPositionEvent += enemyController.SetAiState;
        enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

        //AimStart();
        if (movingAttackCoroutine != null) StopCoroutine(movingAttackCoroutine);
        movingAttackCoroutine = ContinueAttackingWhileMoving();
        StartCoroutine(movingAttackCoroutine);
    }

    IEnumerator CheckLastSeen() {
        for (; ; ) {
            if (Time.time - lastSeen > 60f && enemyController.state != AiState.Chasing) {
                enemyController.state = AiState.Chasing;
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

    IEnumerator ContinueAttackingWhileMoving() {
        for (; ; ) {
            Vector3 eyesPosition = enemyController.eyes.transform.position;
            Ray ray = new Ray(eyesPosition, enemyController.enemy.GetAimPosition().transform.position - eyesPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 40f)) {
                if (hit.collider.gameObject.TryGetComponent<Character>(out Character character)) {
                    if (!aiming) enemyController.AimStart();
                    if (character == enemyController.enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...
                    lastSeen = Time.time;
                    lastKnownPosition = character.transform.position;
                    enemyController.ShootPerformed();
                } else if (enemyController.state != AiState.Chasing) {
                    enemyController.AimCancel();
                    enemyController.RunStart();
                }

            }
            yield return new WaitForSeconds(0.8f);
        }
    }

    IEnumerator ShootingCoroutine() {
        for (; ; ) {
            Vector3 eyesPosition = enemyController.eyes.transform.position;
            Ray ray = new Ray(eyesPosition, enemyController.enemy.GetAimPosition().transform.position - eyesPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 40f)) {
                if (hit.collider.gameObject.TryGetComponent<Character>(out Character character)) {
                    if (!aiming) enemyController.AimStart();
                    if (character == enemyController.enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...
                    lastSeen = Time.time;
                    lastKnownPosition = character.transform.position;
                    enemyController.ShootPerformed();
                }

            }
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        }
        
    }

    IEnumerator ContinueAttackingWhileCovered() {
        bool covered = true;
        if (shootingCoroutine == null) {
            shootingCoroutine = ShootingCoroutine();
            StartCoroutine(shootingCoroutine);
        }
        for (; ; ) {
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            if(covered) {
                enemyController.CrouchCancel();
                enemyController.AimStart();

                Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;

                enemyController.enemy.GetAimPosition().Reposition(pos);
                enemyController.enemy.RotateSelf(coverPosition.transform.forward);
                covered = false;
            } else {
                //StopCoroutine(shootingCoroutine);
                enemyController.CrouchStart();
                enemyController.AimCancel();

                enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
                covered = true;
            }

            yield return new WaitForSeconds(0.8f);
            

        }
    }
}
