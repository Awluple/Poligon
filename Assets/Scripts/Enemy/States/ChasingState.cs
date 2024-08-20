using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections;
using System;
using UnityEngine.TextCore.Text;
using Poligon.EvetArgs;

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
                enemyController.enemy.GetAimPosition().Reposition(enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);
                enemyController.AimStart();
                enemyController.CrouchCancel();
                //enemyController.SetNewDestinaction(coverPosition.transform.position);
                enemyController.SetNewDestinaction(enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);

            } else {
                chasingCoroutine = ChasingCoroutine();
                enemyController.StartCoroutine(chasingCoroutine);
                enemyController.OnFinalPositionEvent += Search;
            }

            enemyController.enemy.GetAimPosition().OnLineOfSight += EnemyFound;
        }
        private void Search(object sender, EventArgs e) {
            enemyController.aiState = AiState.Searching;
            enemyController.OnFinalPositionEvent -= Search;
        }
        public override void ExitState() {
            enemyController.StopCoroutine(movingAttackCoroutine);
            enemyController.enemy.GetAimPosition().OnLineOfSight -= EnemyFound;
        }

        private void EnemyFound(object sender, CharacterEventArgs args) {
            if(args.character != enemyController.attackingLogic.opponent) {
                enemyController.attackingLogic.opponent = args.character;
            }
            //enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
            if(UnityEngine.Random.Range(0, 10) > 6) { enemyController.CrouchStart(); }
            enemyController.SetNewDestinaction(enemyController.transform.position);
            enemyController.aiState = AiState.StationaryAttacking;
        }

        public IEnumerator ChasingCoroutine() {
            for (; ; ) {
                if(enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position != Vector3.zero) {
                    enemyController.SetNewDestinaction(enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);
                    enemyController.RunStart();
                    enemyController.StopCoroutine(chasingCoroutine);
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}