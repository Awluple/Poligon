using UnityEngine;
using System.Collections;

using Poligon.Ai.EnemyStates.Utils;
using Poligon.Ai.Commands;
using System;

namespace Poligon.Ai.EnemyStates {
    public class StationaryAttackState : EnemyBaseState {
        private float timeInState = 0f;
        public override AiState state { get; protected set; } = AiState.StationaryAttacking;

        private IEnumerator shootingCoroutine;

        public StationaryAttackState(EnemyController controller) : base(controller) {
        }
        public override void UpdateState() {
            timeInState += Time.deltaTime;
            if (timeInState > 14f) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
                if (!hasVision) {
                    enemyController.aiState = AiState.Chasing;
                }
                timeInState = 0f;
            }
        }

        private void ResetTimer(object sender, EventArgs args) {
            timeInState = 0f;
        }

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
            enemyController.enemy.OnShoot += ResetTimer;
            enemyController.enemy.OnHealthLoss += TryToHide;

            if (enemyController.enemy.IsCrouching() && !hasVision) {
                enemyController.CrouchCancel();
            }
            if(!enemyController.enemy.IsAiming()) {
                enemyController.AimStart();
            }
            shootingCoroutine = Coroutines.ShootingCoroutine(enemyController);
            enemyController.StartCoroutine(shootingCoroutine);
            if (coverPosition!= null && !hasVision) {
                Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;
                LastKnownPosition opponentPosition = enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent);
                if(opponentPosition != null) {
                    Vector3 aimPosition = (opponentPosition.position == Vector3.zero) ? pos : enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position;

                    enemyController.enemy.GetAimPosition().MoveAim(aimPosition, 80f);
                    enemyController.enemy.RotateSelf(coverPosition.transform.forward);
                }
            }
        }

        public void TryToHide(object sender, EventArgs args) {
            if (enemyController.enemy.health / enemyController.enemy.maxHealth > 0.6) {
                return;
            } else if (UnityEngine.Random.Range(0, 10) > 7) {
                return;
            }
            if (enemyController.hidingLogic.currentCoverPosition != null) {
                enemyController.aiState = AiState.BehindCover;
            } else {
                enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position, enemyController.enemy);
                enemyController.aiState = AiState.Hiding;
            }
        }

        public override void ExitState() {
            enemyController.enemy.OnHealthLoss -= TryToHide;
            if (shootingCoroutine != null) {
                enemyController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            enemyController.enemy.OnShoot -= ResetTimer;
            enemyController.ShootCancel();
        }
    }

}