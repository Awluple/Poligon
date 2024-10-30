using UnityEngine;
using Poligon.Ai.States.Utils;
using System.Collections;
using System;
using UnityEngine.TextCore.Text;
using Poligon.EvetArgs;
using Poligon.Ai.Commands;
using UnityEngine.AI;
using Poligon.Ai.States.Utils;

namespace Poligon.Ai.States {
    public class ChasingState : IndividualBaseState {
        IEnumerator movingAttackCoroutine;
        IEnumerator chasingCoroutine;
        LastKnownPosition lastPosition;
        bool found = false;
        public ChasingState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.Chasing;

        public override void EnterState() {
            //if(Methods.HasAimOnOpponent(out Character character, aiController)) {
            //    aiController.aiState = IndividualAiState.Hiding;
            //    return;
            //}
            if (movingAttackCoroutine != null) aiController.StopCoroutine(movingAttackCoroutine);
            movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(aiController, false);
            aiController.StartCoroutine(movingAttackCoroutine);
            lastPosition = aiController.aiCharacter.squad.GetCharacterLastPosition(aiController.attackingLogic.opponent);
            //if (Methods.HasVisionOnCharacter(out Character hitChar, aiController, lastPosition.character, 20f)) {
            //    aiController.aiState = IndividualAiState.Hiding;
            //    return;
            //}

            if (lastPosition == null) {
                lastPosition = aiController.aiCharacter.squad.GetChasingLocation();
            }
            if (lastPosition == null) { 
                aiController.aiState = IndividualAiState.Searching;
                return;
            }
            if (aiController.hidingLogic.currentCoverSubEdge != null && lastPosition != null) {
                aiController.aiCharacter.GetAimPosition().Reposition(lastPosition.position);
                aiController.AimStart();
                aiController.CrouchCancel();
                //enemyController.SetNewDestinaction(coverPosition.transform.position);
                aiController.SetNewDestinaction(lastPosition.position);

            }
            else { // TODO What to do when noone to chase? Stop? Go back to the leader?
                chasingCoroutine = ChasingCoroutine();
                aiController.StartCoroutine(chasingCoroutine);
                aiController.OnFinalPositionEvent += Search;
            }

            aiController.aiCharacter.GetAimPosition().OnLineOfSight += EnemyFound;
            aiController.attackingLogic.opponent.OnDeath += OpponentDeath;
        }
        private void Search(object sender, EventArgs e) {
            aiController.aiState = IndividualAiState.Searching;
            aiController.OnFinalPositionEvent -= Search;
        }
        private void OpponentDeath(object sender, CharacterEventArgs args) {
            if(aiController.aiCharacter != null && aiController.aiCharacter.destroyCancellationToken.IsCancellationRequested || aiController.attackingLogic.opponent == null) return;
            aiController.attackingLogic.opponent.OnDeath -= OpponentDeath;
            aiController.SetNewDestinaction(aiController.aiCharacter.transform.position);
        }
        public override void UpdateState() {
            if (lastPosition != null && aiController.aiCharacter.IsRunning()) {
                NavMeshPath path = new NavMeshPath();
                aiController.navAgent.CalculatePath(lastPosition.position, path);
                if (Methods.GetPathLength(path) < 10f) {
                    aiController.aiCharacter.GetAimPosition().Reposition(lastPosition.position);
                    if(!aiController.aiCharacter.IsAiming()) aiController.AimStart();
                    if (aiController.aiCharacter.IsRunning()) aiController.RunCancel();
                    if (aiController.aiCharacter.IsCrouching()) aiController.CrouchCancel();
                } 
            }
        }
        public override void ExitState() {
            if(movingAttackCoroutine != null) aiController.StopCoroutine(movingAttackCoroutine);
            aiController.aiCharacter.GetAimPosition().OnLineOfSight -= EnemyFound;
        }

        private void EnemyFound(object sender, CharacterEventArgs args) {
            Debug.Log("test " + found);
            if(found && aiController.aiState != IndividualAiState.Chasing) return;
            found = true;
            aiController.aiCharacter.GetAimPosition().OnLineOfSight -= EnemyFound;
            if (args.character != aiController.attackingLogic.opponent) {
                aiController.attackingLogic.opponent = args.character;
            }
            //enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
            UnityEngine.Random.InitState(aiController.transform.position.GetHashCode());
            var number = UnityEngine.Random.Range(0f, 10f);
            aiController.SetNewDestinaction(aiController.transform.position);
            if (number > 3f) {
                aiController.aiState = IndividualAiState.Hiding;
            } else {
                if (number > 6) aiController.CrouchStart();
                aiController.aiState = IndividualAiState.StationaryAttacking;
            }
            
        }

        public IEnumerator ChasingCoroutine() {
            for (; ; ) {
                LastKnownPosition position = aiController.aiCharacter.squad.GetCharacterLastPosition(aiController.attackingLogic.opponent);
                if (position != null && position.position != Vector3.zero) {
                    lastPosition = position;
                    aiController.SetNewDestinaction(aiController.aiCharacter.squad.GetCharacterLastPosition(aiController.attackingLogic.opponent).position);
                    aiController.AimCancel();
                    aiController.RunStart();
                    aiController.StopCoroutine(chasingCoroutine);
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}