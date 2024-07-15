using System.Collections;
using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Poligon.Ai.EnemyStates {
    public class HidingState : EnemyBaseState {
        private IEnumerator movingAttackCoroutine;
        private IEnumerator attemptHideCoroutine;
        float maxNavMeshSampleDistance = 5f;
        float maxBackOffDistance = 7f;

        public HidingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Hiding;

        public override void EnterState() {
            CoverPosition hidingSpot = enemyController.hidingLogic.currentCoverPosition;

            if (hidingSpot != null && hidingSpot.transform.position != Vector3.zero) {
                enemyController.SetNewDestinaction(hidingSpot.transform.position);
                enemyController.OnFinalPositionEvent += OnPosition;
                enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, true, 0.15f);

            } else {
                
                if(Vector3.Distance(enemyController.transform.position, enemyController.attackingLogic.opponent.transform.position) > 7f) {
                    enemyController.SetNewDestinaction(enemyController.transform.position);
                    if(Random.Range(1, 10) < 8) enemyController.CrouchStart();
                } else {
                    
                    //enemyController.SetNewDestinaction(enemyController.transform.position);
                    MoveAgentAwayFromPoint(enemyController.attackingLogic.opponent.transform.position);
                }
                attemptHideCoroutine = AttemptHideCoroutine();
                enemyController.StartCoroutine(attemptHideCoroutine);
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, false);
            }


            enemyController.StartCoroutine(movingAttackCoroutine);
        }

        private IEnumerator AttemptHideCoroutine() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, enemyController)) {
                    enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position);
                } else {
                    enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position);
                }

                CoverPosition hidingSpot = enemyController.hidingLogic.currentCoverPosition;
                if (hidingSpot != null && hidingSpot.transform.position != Vector3.zero) {
                    enemyController.CrouchCancel();

                    enemyController.SetNewDestinaction(hidingSpot.transform.position);
                    enemyController.OnFinalPositionEvent += OnPosition;
                    enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

                    enemyController.StopCoroutine(attemptHideCoroutine);
                }


                yield return new WaitForSeconds(3f);
            }
        }

        private void OnPosition(object sender = null, System.EventArgs e = null) {
            SetBehindCoverPosition();
        }
        public override void ExitState() {
            if (movingAttackCoroutine != null) enemyController.StopCoroutine(movingAttackCoroutine);
            if (attemptHideCoroutine != null) enemyController.StopCoroutine(attemptHideCoroutine);
            
            enemyController.OnFinalPositionEvent -= OnPosition;
            enemyController.OnFinalPositionEvent -= (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

            enemyController.ShootCancel();
        }
        public void SetBehindCoverPosition() {

            if (!Methods.HasAimOnOpponent(out Character chara, enemyController)) {
                enemyController.enemy.RotateSelf(-enemyController.hidingLogic.currentCoverPosition.transform.position);
                List<CoverPose> poses = enemyController.hidingLogic.currentCoverPosition.GetCoverPoses();
                if (poses.Count == 1) {

                    if (poses[0] == CoverPose.Standing) {
                        enemyController.CrouchStart();
                    }
                }
                enemyController.aiState = AiState.BehindCover;
                //coveredAttackCoroutine = ContinueAttackingWhileCovered();
                //enemyController.StartCoroutine(coveredAttackCoroutine);
            } else {
                enemyController.aiState = AiState.Attacking;
            }
        }
        void MoveAgentAwayFromPoint(Vector3 avoidPoint) {
            // Calculate direction away from the point
            Vector3 directionAway = (enemyController.navAgent.transform.position - avoidPoint).normalized;

            // Determine the target position away from the point
            Vector3 targetPosition = enemyController.navAgent.transform.position + directionAway * maxBackOffDistance;

            // Find a valid position on the NavMesh near the target position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, maxNavMeshSampleDistance, NavMesh.AllAreas)) {
                if(IsPathWithinDistance(hit.position, maxBackOffDistance)) {
                    enemyController.SetNewDestinaction(hit.position);
                } else {
                    TryDifferentDirection(avoidPoint);
                }
                
            } else {
                TryDifferentDirection(avoidPoint);
            }
        }
        bool IsPathWithinDistance(Vector3 targetPosition, float maxDistance) {
            NavMeshPath path = new NavMeshPath();
            enemyController.navAgent.CalculatePath(targetPosition, path);
            float pathLength = GetPathLength(path);
            return pathLength <= maxDistance;
        }
        float GetPathLength(NavMeshPath path) {
            float length = 0;
            if (path.corners.Length < 2) return length;
            for (int i = 1; i < path.corners.Length; i++) {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return length;
        }
        void TryDifferentDirection(Vector3 avoidPoint) {
            // If the straight line approach fails, try other directions
            float angleIncrement = 30f; // degrees to increment
            for (float angle = angleIncrement; angle < 360f; angle += angleIncrement) {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * (enemyController.navAgent.transform.position - avoidPoint).normalized;
                Vector3 targetPosition = enemyController.navAgent.transform.position + direction * maxBackOffDistance;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, maxNavMeshSampleDistance, NavMesh.AllAreas)) {
                    if (IsPathWithinDistance(hit.position, maxBackOffDistance)) {
                        enemyController.SetNewDestinaction(hit.position);
                        break;
                    }
                }
            }
        }
    }
}