using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Poligon.Ai.EnemyStates;
using Poligon.Ai.EnemyStates.Utils;



public class AttackingLogic : MonoBehaviour
{

    private EnemyController enemyController;

    private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator keepTrackOnEnemyCoroutine;


    public Character opponent;

    private float lastSeen;
    public Vector3 lastKnownPosition { get; private set; } = Vector3.zero;

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }

    public void SetBehindCoverPosition() {

        if(!Methods.HasVisionOnOpponent(out Character chara, enemyController)) {
            enemyController.enemy.RotateSelf(-enemyController.hidingLogic.currentCoverPosition.transform.position);
            List<CoverPose> poses = enemyController.hidingLogic.currentCoverPosition.GetCoverPoses();
            if (poses.Count == 1) {

                if (poses[0] == CoverPose.Standing) {
                    enemyController.CrouchStart();
                }
            }
            enemyController.aiState = AiState.BehindCover;
        } else {
            enemyController.aiState = AiState.Attacking;
        }

        coveredAttackCoroutine = ContinueAttackingWhileCovered();
        StartCoroutine(coveredAttackCoroutine);
    }

    private void StopAttacking(object sender = null, System.EventArgs e = null) {
        if (checkLastSeen != null) StopCoroutine(checkLastSeen);
        enemyController.AimCancel();
        enemyController.aiState = AiState.Patrolling;
        enemyController.currentPatrolPosition = -1;
        enemyController.SetPatrollingPath();
    }

    public void EnemySpotted() {
        
        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        checkLastSeen = CheckLastSeen();
        StartCoroutine(checkLastSeen);

        Player player = FindFirstObjectByType<Player>();

        enemyController.hidingLogic.GetHidingPosition(player.transform.position);

        keepTrackOnEnemyCoroutine = KeepTrackOnEnemy();
        StartCoroutine(keepTrackOnEnemyCoroutine);

        enemyController.aiState = AiState.Hiding;
    }

    IEnumerator CheckLastSeen() {
        for (; ; ) {
            if (Time.time - lastSeen > 50f && enemyController.aiState != AiState.Chasing) {
                enemyController.aiState = AiState.Chasing;
                enemyController.SetNewDestinaction(lastKnownPosition);

                enemyController.RunCancel();
                enemyController.enemy.GetAimPosition().Reset();
            } else if (Time.time - lastSeen > 60f) {
                StopAttacking();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator KeepTrackOnEnemy() {
        bool needsReposition = false;
        for (; ; ) {
            Character character = null;
            bool hasVis = Methods.HasVisionOnOpponent(out character, enemyController);
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

        for (; ; ) {
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            if(Methods.HasVisionOnOpponent(out Character character, enemyController) ) {
                lastSeenIteration = 0;
                goto End;
            }
            lastSeenIteration++;
            if (lastSeenIteration == 5) {
                enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position, enemyController.enemy.GetAimPosition().transform.position, true, true, 3f, 30f, 11f);
                enemyController.aiState = AiState.Chasing;
                StopCoroutine(coveredAttackCoroutine);

                lastSeen = 30f;
                goto End;
            }


            if (covered) {
                enemyController.aiState = AiState.Attacking;
                covered = false;
            } else {
                enemyController.aiState = AiState.BehindCover;
                covered = true;
            }
            End:;

        }
    }
}
