using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Poligon.Ai.EnemyStates;
using Poligon.Ai.EnemyStates.Utils;



public class AttackingLogic : MonoBehaviour {

    private EnemyController enemyController;

    private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator keepTrackOnEnemyCoroutine;


    public Character opponent;

    public float enemySinceLastSeen { get; private set; }

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }
    private void UpdateLastKnownPosition(Vector3 newPosition) {
        enemyController.enemy.squad.lastKnownPosition = newPosition;
    }
    public void SetBehindCoverPosition() {

        if(!Methods.HasAimOnOpponent(out Character chara, enemyController)) {
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

    public void EnemySpotted(Character character) {
        
        if(Methods.HasVisionOnCharacter(out Character hitChar, enemyController, character, 70f)) {
            enemyController.hidingLogic.GetHidingPosition(character.transform.position);
            enemyController.aiState = AiState.Hiding;
            StartTrackCoroutine();
            Debug.DrawRay(enemyController.eyes.transform.position, character.transform.position - enemyController.eyes.transform.position, Color.magenta, 2f);
        } else {
            enemyController.aiState = AiState.Chasing;
        }

        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        checkLastSeen = CheckLastSeen();
        StartCoroutine(checkLastSeen);


    }
    public void StartTrackCoroutine() {
        keepTrackOnEnemyCoroutine = KeepTrackOnEnemy();
        StartCoroutine(keepTrackOnEnemyCoroutine);
    }

    IEnumerator CheckLastSeen() {
        for (; ; ) {
            if (Time.time - enemySinceLastSeen > 50f && enemyController.aiState != AiState.Chasing) {
                enemyController.aiState = AiState.Chasing;
                enemyController.SetNewDestinaction(enemyController.enemy.squad.lastKnownPosition);

                enemyController.RunCancel();
                enemyController.enemy.GetAimPosition().Reset();
            } else if (Time.time - enemySinceLastSeen > 60f) {
                //StopAttacking();
                enemyController.aiState = AiState.Searching;
                StopCoroutine(checkLastSeen);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator KeepTrackOnEnemy() {
        bool needsReposition = false;
        for (; ; ) {
            Character character = null;
            bool hasVis = Methods.HasAimOnOpponent(out character, enemyController);
            if (hasVis) {
                UpdateLastKnownPosition(character.transform.position);
                enemySinceLastSeen = Time.time;
                needsReposition = true;
            } else {
                
                if (needsReposition) enemyController.enemy.GetAimPosition().Reposition(enemyController.enemy.squad.lastKnownPosition);
                needsReposition = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ContinueAttackingWhileCovered() {

        for (; ; ) {
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            if(Methods.HasAimOnOpponent(out Character character, enemyController) ) {
                if(enemyController.aiState != AiState.Attacking) {
                    enemyController.aiState = AiState.Attacking;
                }
                goto End;
            }

            if (enemyController.aiState == AiState.BehindCover && (Time.time - enemySinceLastSeen > 30f)) {
                enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position, enemyController.enemy.GetAimPosition().transform.position, true, true, 3f, 30f, 11f);
                enemyController.aiState = AiState.Chasing;
                StopCoroutine(coveredAttackCoroutine);

                goto End;
            }


            if (enemyController.aiState == AiState.BehindCover) {
                enemyController.aiState = AiState.Attacking;
            } else {
                enemyController.aiState = AiState.BehindCover;
            }
            End:;

        }
    }
}
