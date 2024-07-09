using UnityEngine;

namespace Poligon.Ai.EnemyStates {
    public class BehindCoverState : EnemyBaseState {
        public BehindCoverState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.BehindCover;

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            enemyController.ShootCancel();
            enemyController.CrouchStart();
            enemyController.AimCancel();
            enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
        }
    }
}