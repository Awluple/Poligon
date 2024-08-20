using Poligon.Ai.EnemyStates.Utils;
using System.Collections;
using UnityEngine;

namespace Poligon.Ai.EnemyStates {
    public class BehindCoverState : EnemyBaseState {
        IEnumerator checkVisionCoroutine;
        private IEnumerator coveredAttackCoroutine;
        public BehindCoverState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.BehindCover;

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            enemyController.ShootCancel();
            if (!enemyController.enemy.IsCrouching()) {
                enemyController.CrouchStart();
                enemyController.enemy.RotateSelf(-coverPosition.transform.forward);
            }
            enemyController.AimCancel();
            enemyController.OnHealthLoss += HealthLost;
            checkVisionCoroutine = CheckVision();
            enemyController.StartCoroutine(checkVisionCoroutine);
            coveredAttackCoroutine = ContinueAttackingWhileCovered();
            enemyController.StartCoroutine(coveredAttackCoroutine);

        }
        public override void ExitState() {
            enemyController.OnHealthLoss -= HealthLost;
            if (checkVisionCoroutine != null) enemyController.StopCoroutine(checkVisionCoroutine);
            if(coveredAttackCoroutine != null) enemyController.StopCoroutine(coveredAttackCoroutine);
        }

        private void HealthLost(object sender, BulletDataEventArgs eventArgs) {
            enemyController.aiState = AiState.StationaryAttacking;
        }
        private IEnumerator CheckVision() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, enemyController, 40f)) {
                    enemyController.aiState = AiState.StationaryAttacking;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        IEnumerator ContinueAttackingWhileCovered() {

            for (; ; ) {
                yield return new WaitForSeconds(Random.Range(4f, 8f));
                enemyController.aiState = AiState.StationaryAttacking;
                //    if (Methods.HasAimOnOpponent(out Character character, enemyController)) {
                //        enemyController.aiState = AiState.Attacking;
                //        goto End;
                //    }

                //    if (enemyController.aiState == AiState.BehindCover && (Time.time - enemyController.attackingLogic.enemySinceLastSeen > 30f)) {
                //        enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position, enemyController.enemy.GetAimPosition().transform.position, true, true, 3f, 30f, 11f);
                //        enemyController.aiState = AiState.Chasing;
                //        enemyController.StopCoroutine(coveredAttackCoroutine);

                //        goto End;
                //    }



                //End:;

            }
        }
    }
}