using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections;

namespace Poligon.Ai.EnemyStates {
    public class ChasingState : EnemyBaseState {
        IEnumerator movingAttackCoroutine;

        public ChasingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Chasing;

        public override void EnterState() {
            CoverPosition coverPosition = enemyController.hidingLogic.currentCoverPosition;
            if (coverPosition.transform.position != Vector3.zero) {
                enemyController.enemy.GetAimPosition().Reposition(enemyController.GetOpponentLastKnownPosition());
                enemyController.AimStart();
                enemyController.CrouchCancel();

                if (movingAttackCoroutine != null) enemyController.StopCoroutine(movingAttackCoroutine);
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, false);
                enemyController.StartCoroutine(movingAttackCoroutine);

                enemyController.SetNewDestinaction(coverPosition.transform.position);
            }
        }
        public override void ExitState() {
            enemyController.StopCoroutine(movingAttackCoroutine);
        }
    }
}