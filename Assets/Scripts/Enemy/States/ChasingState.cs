using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections;
using System;
using UnityEngine.TextCore.Text;

namespace Poligon.Ai.EnemyStates {
    public class ChasingState : EnemyBaseState {
        IEnumerator movingAttackCoroutine;
        IEnumerator chasingCoroutine;

        public ChasingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Chasing;

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;

            if (movingAttackCoroutine != null) enemyController.StopCoroutine(movingAttackCoroutine);
            movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, false);
            enemyController.StartCoroutine(movingAttackCoroutine);

            if (coverPosition != null && coverPosition.transform.position != Vector3.zero) {
                enemyController.enemy.GetAimPosition().Reposition(enemyController.enemy.squad.GetChasingLocation().position);
                enemyController.AimStart();
                enemyController.CrouchCancel();
                enemyController.SetNewDestinaction(coverPosition.transform.position);

            } else {
                chasingCoroutine = ChasingCoroutine();
                enemyController.StartCoroutine(chasingCoroutine);
                enemyController.OnFinalPositionEvent += Search;
            }

            enemyController.enemy.GetAimPosition().OnLineOfSight += Hide;
        }
        private void Search(object sender, EventArgs e) {
            enemyController.aiState = AiState.Searching;
            enemyController.OnFinalPositionEvent -= Search;
        }
        public override void ExitState() {
            enemyController.StopCoroutine(movingAttackCoroutine);
            enemyController.enemy.GetAimPosition().OnLineOfSight -= Hide;
        }

        private void Hide(object sender, EventArgs args) {
            enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position);
            enemyController.attackingLogic.StartTrackCoroutine();
            enemyController.aiState = AiState.Hiding;
        }

        public IEnumerator ChasingCoroutine() {
            for (; ; ) {
                if(enemyController.enemy.squad.GetChasingLocation().position != Vector3.zero) {
                    enemyController.SetNewDestinaction(enemyController.enemy.squad.GetChasingLocation().position);
                    enemyController.RunStart();
                    enemyController.StopCoroutine(chasingCoroutine);
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}