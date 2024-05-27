using UnityEngine;
using System.Collections;

using Poligon.Ai.EnemyStates.Utils;

namespace Poligon.Ai.EnemyStates {
    public class AttackingState : EnemyBaseState {
        public override AiState state { get; protected set; } = AiState.Attacking;

        private IEnumerator shootingCoroutine;

        public AttackingState(EnemyController controller) : base(controller) {
        }

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;

            shootingCoroutine = Coroutines.ShootingCoroutine(enemyController);
            enemyController.StartCoroutine(shootingCoroutine);
            if(enemyController.enemy.IsCrouching()) {
                enemyController.CrouchCancel();
            }
            if(!enemyController.enemy.IsAiming()) {
                enemyController.AimStart();
            }
            if(coverPosition!= null) {
                Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;

                enemyController.enemy.GetAimPosition().Reposition(enemyController.enemy.squad.lastKnownPosition == Vector3.zero ? pos : enemyController.enemy.squad.lastKnownPosition);
                enemyController.enemy.RotateSelf(coverPosition.transform.forward);
            }
        }

        public override void ExitState() {
            if (shootingCoroutine != null) {
                enemyController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
        }
    }

}