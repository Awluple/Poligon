using System.Collections;
using UnityEngine;

namespace Poligon.Ai.States {
    public class PatrollingState : IndividualBaseState {
        public PatrollingState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.Patrolling;

        public int currentPatrolPosition = -1;
        private IEnumerator pathRecalcCoroutine;
        private IEnumerator waitInPatrolPositionCoroutine;


        public override void EnterState() {
            SetPatrollingPath();
        }
        public override void ExitState() {
            if(waitInPatrolPositionCoroutine != null) {
                aiController.StopCoroutine(waitInPatrolPositionCoroutine);
            }
        }

        public void SetPatrollingPath(object sender = null, System.EventArgs e = null) {
            if (aiController.patrolPositions.Length == 0) {
                aiController.onFinalPosition = true;

                return;
            };
            if ((currentPatrolPosition == -1 || aiController.onFinalPosition) && aiController.aiState == IndividualAiState.Patrolling) {
                waitInPatrolPositionCoroutine = WaitInPatrollingPosition();
                aiController.StartCoroutine(waitInPatrolPositionCoroutine);
            }
        }

        private void patrolPointSelection() {
            System.Random random = new System.Random();
            currentPatrolPosition = random.Next(0, aiController.patrolPositions.Length);
            aiController.SetNewDestinaction(aiController.patrolPositions[currentPatrolPosition].transform.position);
            aiController.OnFinalPositionEvent += SetPatrollingPath;
        }
        private IEnumerator RecalculatePath() {
            while (!aiController.onFinalPosition) {
                aiController.currentCorner = 1;
                aiController.navAgent.CalculatePath(aiController.destination.corners[aiController.destination.corners.Length - 1], aiController.destination);
                yield return new WaitForSeconds(.5f);
            }
        }
        private IEnumerator WaitInPatrollingPosition() {
            System.Random random = new System.Random();
            int waitTime = random.Next(3, 6);
            yield return new WaitForSeconds(waitTime);

            patrolPointSelection();
            if (pathRecalcCoroutine != null) aiController.StopCoroutine(pathRecalcCoroutine);
            pathRecalcCoroutine = RecalculatePath();
            aiController.StartCoroutine(pathRecalcCoroutine);
            aiController.StopCoroutine(waitInPatrolPositionCoroutine);
        }
    }
}