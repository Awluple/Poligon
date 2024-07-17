using UnityEngine;
using System.Collections;

using Poligon.Ai.EnemyStates.Utils;

namespace Poligon.Ai.EnemyStates {
    public class AttackingState : EnemyBaseState {
        private float timeInState = 0f;
        public override AiState state { get; protected set; } = AiState.Attacking;

        private IEnumerator shootingCoroutine;

        public AttackingState(EnemyController controller) : base(controller) {
        }
        public override void UpdateState() {
            //timeInState += Time.deltaTime;
            //if(timeInState > 6f) {
            //    bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
            //    if(!hasVision) {
            //        enemyController.aiState = AiState.BehindCover;
            //    }
            //    timeInState = 0f;
            //}
        }
        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController, 40f);
            
            if(enemyController.enemy.IsCrouching() && !hasVision) {
                enemyController.CrouchCancel();
            }
            if(!enemyController.enemy.IsAiming()) {
                enemyController.AimStart();
            }
            shootingCoroutine = Coroutines.ShootingCoroutine(enemyController);
            enemyController.StartCoroutine(shootingCoroutine);
            if (coverPosition!= null && !hasVision) {
                Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;
                Vector3 opponentPosition = enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position;
                Vector3 aimPosition = (opponentPosition == Vector3.zero) ? pos : enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position;
                
                enemyController.enemy.GetAimPosition().Reposition(aimPosition);
                enemyController.enemy.RotateSelf(coverPosition.transform.forward);
            }
        }

        public override void ExitState() {
            if (shootingCoroutine != null) {
                enemyController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            enemyController.ShootCancel();
        }
    }

}