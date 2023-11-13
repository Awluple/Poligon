using System.Collections;
using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;

namespace Poligon.Ai.EnemyStates {
    public class HidingState : EnemyBaseState {
        private IEnumerator movingAttackCoroutine;


        public HidingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Hiding;

        public override void EnterState() {

            Vector3 hidingSpot = enemyController.hidingLogic.currentCoverPosition.transform.position;


            if (hidingSpot != Vector3.zero) {
                enemyController.SetNewDestinaction(hidingSpot);
                enemyController.OnFinalPositionEvent += OnPosition;
                enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

            } else {
                enemyController.SetNewDestinaction(enemyController.transform.position);
                enemyController.CrouchStart();
            }

            movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController);
            enemyController.StartCoroutine(movingAttackCoroutine);
        }

        private void OnPosition(object sender = null, System.EventArgs e = null) {
            enemyController.aiState = AiState.BehindCover;
            enemyController.attackingLogic.SetBehindCoverPosition();
        }
        public override void ExitState() {
            enemyController.StopCoroutine(movingAttackCoroutine);
        }
    }
}