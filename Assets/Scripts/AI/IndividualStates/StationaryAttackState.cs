using UnityEngine;
using System.Collections;

using Poligon.Ai.States.Utils;
using Poligon.Ai.Commands;
using System;

namespace Poligon.Ai.States {
    public class StationaryAttackState : IndividualBaseState {
        private float timeInState = 0f;
        public override IndividualAiState state { get; protected set; } = IndividualAiState.StationaryAttacking;

        private IEnumerator shootingCoroutine;

        public StationaryAttackState(AiCharacterController controller) : base(controller) {
        }
        public override void UpdateState() {
            if(aiController.aiState != state) return;
            timeInState += Time.deltaTime;
            if (timeInState > 9f) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, aiController, 40f);
                int enemies = aiController.aiCharacter.squad.knownEnemies.Count;
                if ((!hasVision && enemies > 0) || aiController.hidingLogic.currentCoverSubEdge == null) {
                    aiController.aiState = IndividualAiState.Hiding;
                } else {
                    //aiController.aiState = IndividualAiState.Searching;
                }
                timeInState = 0f;
            }
        }

        private void ResetTimer(object sender, EventArgs args) {
            timeInState = 0f;
        }

        public override void EnterState() {
            aiController.SetNewDestinaction(aiController.aiCharacter.transform.position);
            bool hasVision = Methods.HasAimOnOpponent(out Character character, aiController, 40f);
            aiController.aiCharacter.OnShoot += ResetTimer;
            aiController.aiCharacter.OnHealthLoss += TryToHide;

            if (aiController.aiCharacter.IsCrouching() && !hasVision) {
                aiController.CrouchCancel();
            }
            if(!aiController.aiCharacter.IsAiming()) {
                aiController.AimStart();
            }
            shootingCoroutine = Coroutines.ShootingCoroutine(aiController);
            aiController.StartCoroutine(shootingCoroutine);
            if (aiController.hidingLogic.currentCoverSubEdge != null && !hasVision) {
                Vector3 pos = aiController.hidingLogic.currentCoverEdge.forward * 4 + aiController.eyes.transform.position;
                LastKnownPosition opponentPosition = aiController.aiCharacter.squad.GetCharacterLastPosition(aiController.attackingLogic.opponent);
                if(opponentPosition != null) {
                    Vector3 aimPosition = (opponentPosition.position == Vector3.zero) ? pos : aiController.aiCharacter.squad.GetCharacterLastPosition(aiController.attackingLogic.opponent).position;

                    aiController.aiCharacter.GetAimPosition().MoveAim(aimPosition, 80f);
                    aiController.aiCharacter.RotateSelf(aiController.hidingLogic.currentCoverEdge.forward);
                }
            }
        }

        public void TryToHide(object sender, EventArgs args) {
            if (aiController.aiCharacter.health / aiController.aiCharacter.maxHealth > 0.6) {
                return;
            } else if (UnityEngine.Random.Range(0, 10) > 7) {
                return;
            }
            if (aiController.hidingLogic.currentCoverSubEdge != null) {
                aiController.aiState = IndividualAiState.BehindCover;
            } else {
                //enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
                aiController.aiState = IndividualAiState.Hiding;
            }
        }

        public override void ExitState() {
            aiController.aiCharacter.OnHealthLoss -= TryToHide;
            if (shootingCoroutine != null) {
                aiController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            aiController.aiCharacter.OnShoot -= ResetTimer;
            aiController.ShootCancel();
        }
    }

}