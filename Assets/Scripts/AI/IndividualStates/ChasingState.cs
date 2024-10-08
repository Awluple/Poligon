using UnityEngine;
using Poligon.Ai.States.Utils;
using System.Collections;
using System;
using UnityEngine.TextCore.Text;
using Poligon.EvetArgs;

namespace Poligon.Ai.States {
    public class ChasingState : IndividualBaseState {
        IEnumerator movingAttackCoroutine;
        IEnumerator chasingCoroutine;

        public ChasingState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.Chasing;

        public override void EnterState() {

            if (movingAttackCoroutine != null) enemyController.StopCoroutine(movingAttackCoroutine);
            movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, false);
            enemyController.StartCoroutine(movingAttackCoroutine);

            if (enemyController.hidingLogic.currentCoverSubEdge != null) {
                enemyController.aiCharacter.GetAimPosition().Reposition(enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);
                enemyController.AimStart();
                enemyController.CrouchCancel();
                //enemyController.SetNewDestinaction(coverPosition.transform.position);
                enemyController.SetNewDestinaction(enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);

            } else {
                chasingCoroutine = ChasingCoroutine();
                enemyController.StartCoroutine(chasingCoroutine);
                enemyController.OnFinalPositionEvent += Search;
            }

            enemyController.aiCharacter.GetAimPosition().OnLineOfSight += EnemyFound;
        }
        private void Search(object sender, EventArgs e) {
            enemyController.aiState = IndividualAiState.Searching;
            enemyController.OnFinalPositionEvent -= Search;
        }
        public override void ExitState() {
            enemyController.StopCoroutine(movingAttackCoroutine);
            enemyController.aiCharacter.GetAimPosition().OnLineOfSight -= EnemyFound;
        }

        private void EnemyFound(object sender, CharacterEventArgs args) {
            if(args.character != enemyController.attackingLogic.opponent) {
                enemyController.attackingLogic.opponent = args.character;
            }
            //enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
            if(UnityEngine.Random.Range(0, 10) > 6) { enemyController.CrouchStart(); }
            enemyController.SetNewDestinaction(enemyController.transform.position);
            enemyController.aiState = IndividualAiState.StationaryAttacking;
        }

        public IEnumerator ChasingCoroutine() {
            for (; ; ) {
                if(enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position != Vector3.zero) {
                    enemyController.SetNewDestinaction(enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position);
                    enemyController.AimCancel();
                    enemyController.RunStart();
                    enemyController.StopCoroutine(chasingCoroutine);
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}