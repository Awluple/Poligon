using UnityEngine;
using System.Collections;
using Poligon.Ai.EnemyStates.Utils;

namespace Poligon.Ai.EnemyStates {
    public class BehindCoverState : EnemyBaseState {
        IEnumerator checkVisionCoroutine;
        public BehindCoverState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.BehindCover;

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            enemyController.ShootCancel();
            if(!enemyController.enemy.IsCrouching()) {
                enemyController.CrouchStart();
                enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
            }
            enemyController.AimCancel();
            enemyController.OnHealthLoss += HealthLost;
            checkVisionCoroutine = CheckVision();
            enemyController.StartCoroutine(checkVisionCoroutine);
            
        }
        public override void ExitState() {
            enemyController.OnHealthLoss -= HealthLost;
            enemyController.StopCoroutine(checkVisionCoroutine);
        }

        private void HealthLost(object sender, BulletDataEventArgs eventArgs) {
            enemyController.aiState = AiState.Attacking;
        }
        private IEnumerator CheckVision() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, enemyController, 40f)) {
                    enemyController.aiState = AiState.Attacking;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}