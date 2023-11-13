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

            enemyController.CrouchCancel();
            enemyController.AimStart();
            Vector3 pos = coverPosition.transform.forward * 4 + enemyController.eyes.transform.position;

            enemyController.enemy.GetAimPosition().Reposition(enemyController.GetOpponentLastKnownPosition() == Vector3.zero ? pos : enemyController.GetOpponentLastKnownPosition());
            enemyController.enemy.RotateSelf(coverPosition.transform.forward);
        }

        public override void ExitState() {
            if (shootingCoroutine != null) {
                enemyController.StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
        }
    }

}