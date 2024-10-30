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
            aiController.ShootCancel();
            if (!aiController.aiCharacter.IsCrouching()) {
                aiController.CrouchStart();
                aiController.aiCharacter.RotateSelf(-aiController.hidingLogic.currentCoverEdge.forward);
            }
            aiController.AimCancel();
            aiController.OnHealthLoss += HealthLost;
            checkVisionCoroutine = CheckVision();
            aiController.StartCoroutine(checkVisionCoroutine);
            coveredAttackCoroutine = ContinueAttackingWhileCovered();
            aiController.StartCoroutine(coveredAttackCoroutine);

        }
        public override void ExitState() {
            aiController.OnHealthLoss -= HealthLost;
            if (checkVisionCoroutine != null) aiController.StopCoroutine(checkVisionCoroutine);
            if(coveredAttackCoroutine != null) aiController.StopCoroutine(coveredAttackCoroutine);
        }

        private void HealthLost(object sender, BulletDataEventArgs eventArgs) {
            aiController.aiState = IndividualAiState.StationaryAttacking;
        }
        private IEnumerator CheckVision() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, aiController, 40f)) {
                    aiController.aiState = IndividualAiState.StationaryAttacking;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        IEnumerator ContinueAttackingWhileCovered() {

            for (; ; ) {
                yield return new WaitForSeconds(Random.Range(4f, 8f));
                if (aiController.aiState == IndividualAiState.BehindCover)aiController.aiState = IndividualAiState.StationaryAttacking;
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