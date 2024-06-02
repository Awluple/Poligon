using System.Collections;
using UnityEngine;

namespace Poligon.Ai.EnemyStates {
    public class PatrollingState : EnemyBaseState {
        public PatrollingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Patrolling;

        public int currentPatrolPosition = -1;
        private IEnumerator pathRecalcCoroutine;
        private IEnumerator waitInPatrolPositionCoroutine;


        public override void EnterState() {
            SetPatrollingPath();
        }
        public override void ExitState() {
            if(waitInPatrolPositionCoroutine != null) {
                enemyController.StopCoroutine(waitInPatrolPositionCoroutine);
            }
        }

        public void SetPatrollingPath(object sender = null, System.EventArgs e = null) {
            if (enemyController.patrolPositions.Length == 0) {
                enemyController.onFinalPosition = true;

                return;
            };
            if ((currentPatrolPosition == -1 || enemyController.onFinalPosition) && enemyController.aiState == AiState.Patrolling) {
                waitInPatrolPositionCoroutine = WaitInPatrollingPosition();
                enemyController.StartCoroutine(waitInPatrolPositionCoroutine);
            }
        }

        private void patrolPointSelection() {
            System.Random random = new System.Random();
            currentPatrolPosition = random.Next(0, enemyController.patrolPositions.Length);
            enemyController.SetNewDestinaction(enemyController.patrolPositions[currentPatrolPosition].transform.position);
            enemyController.OnFinalPositionEvent += SetPatrollingPath;
        }
        private IEnumerator RecalculatePath() {
            while (!enemyController.onFinalPosition) {
                enemyController.currentCorner = 1;
                enemyController.navAgent.CalculatePath(enemyController.destination.corners[enemyController.destination.corners.Length - 1], enemyController.destination);
                yield return new WaitForSeconds(.5f);
            }
        }
        private IEnumerator WaitInPatrollingPosition() {
            System.Random random = new System.Random();
            int waitTime = random.Next(3, 6);
            yield return new WaitForSeconds(waitTime);

            patrolPointSelection();
            if (pathRecalcCoroutine != null) enemyController.StopCoroutine(pathRecalcCoroutine);
            pathRecalcCoroutine = RecalculatePath();
            enemyController.StartCoroutine(pathRecalcCoroutine);
            enemyController.StopCoroutine(waitInPatrolPositionCoroutine);
        }
    }
}