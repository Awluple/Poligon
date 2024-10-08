using Poligon.Ai.States.Utils;
using System.Collections;
using UnityEngine;

namespace Poligon.Ai.States {
    public class BehindCoverState : IndividualBaseState {
        IEnumerator checkVisionCoroutine;
        private IEnumerator coveredAttackCoroutine;
        public BehindCoverState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.BehindCover;

        public override void EnterState() {
            enemyController.ShootCancel();
            if (!enemyController.aiCharacter.IsCrouching()) {
                enemyController.CrouchStart();
                enemyController.aiCharacter.RotateSelf(-enemyController.hidingLogic.currentCoverEdge.forward);
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
            enemyController.aiState = IndividualAiState.StationaryAttacking;
        }
        private IEnumerator CheckVision() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, enemyController, 40f)) {
                    enemyController.aiState = IndividualAiState.StationaryAttacking;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        IEnumerator ContinueAttackingWhileCovered() {

            for (; ; ) {
                yield return new WaitForSeconds(Random.Range(4f, 8f));
                if(enemyController.aiState == IndividualAiState.BehindCover)enemyController.aiState = IndividualAiState.StationaryAttacking;
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