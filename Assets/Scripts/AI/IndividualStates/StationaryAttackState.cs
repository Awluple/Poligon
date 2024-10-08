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
            timeInState += Time.deltaTime;
            if (timeInState > 14f) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
                if (!hasVision) {
                    enemyController.aiState = IndividualAiState.Chasing;
                }
                timeInState = 0f;
            }
        }

        private void ResetTimer(object sender, EventArgs args) {
            timeInState = 0f;
        }

        public override void EnterState() {
            bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
            enemyController.aiCharacter.OnShoot += ResetTimer;
            enemyController.aiCharacter.OnHealthLoss += TryToHide;

            if (enemyController.aiCharacter.IsCrouching() && !hasVision) {
                enemyController.CrouchCancel();
            }
            if(!enemyController.aiCharacter.IsAiming()) {
                enemyController.AimStart();
            }
            shootingCoroutine = Coroutines.ShootingCoroutine(enemyController);
            enemyController.StartCoroutine(shootingCoroutine);
            if (enemyController.hidingLogic.currentCoverSubEdge != null && !hasVision) {
                Vector3 pos = enemyController.hidingLogic.currentCoverEdge.forward * 4 + enemyController.eyes.transform.position;
                LastKnownPosition opponentPosition = enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent);
                if(opponentPosition != null) {
                    Vector3 aimPosition = (opponentPosition.position == Vector3.zero) ? pos : enemyController.aiCharacter.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position;

                    enemyController.aiCharacter.GetAimPosition().MoveAim(aimPosition, 80f);
                    enemyController.aiCharacter.RotateSelf(enemyController.hidingLogic.currentCoverEdge.forward);
                }
            }
        }

        public void TryToHide(object sender, EventArgs args) {
            if (enemyController.aiCharacter.health / enemyController.aiCharacter.maxHealth > 0.6) {
                return;
            } else if (UnityEngine.Random.Range(0, 10) > 7) {
                return;
            }
            if (enemyController.hidingLogic.currentCoverSubEdge != null) {
                enemyController.aiState = IndividualAiState.BehindCover;
            } else {
                //enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
                enemyController.aiState = IndividualAiState.Hiding;
            }
        }

        public override void ExitState() {
            enemyController.aiCharacter.OnHealthLoss -= TryToHide;
            if (shootingCoroutine != null) {
                enemyController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            enemyController.aiCharacter.OnShoot -= ResetTimer;
            enemyController.ShootCancel();
        }
    }

}