using System.Collections;
using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Poligon.Ai.EnemyStates {
    public class HidingState : EnemyBaseState {
        private IEnumerator movingAttackCoroutine;
        private IEnumerator attemptHideCoroutine;
        float timeSinceLastSeen = Time.time;
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
                    enemyController.enemy.GetAimPosition().LockOnTarget(enemyController.attackingLogic.opponent);
                } else {
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
                bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController);
                if (hasVision) {
                    enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position);
                    timeSinceLastSeen = Time.time;
                } else {
                    if(Time.time - timeSinceLastSeen > 5.9f) {
                        enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.squad.GetCharacterLastPosition(enemyController.attackingLogic.opponent).position, null, true, false, 8f, 30f, 32f);
                    }
                }

                CoverPosition hidingSpot = enemyController.hidingLogic.currentCoverPosition;
                if (hidingSpot != null && hidingSpot.transform.position != Vector3.zero) {
                    enemyController.CrouchCancel();
                    enemyController.SetNewDestinaction(hidingSpot.transform.position);
                    enemyController.OnFinalPositionEvent += OnPosition;
                    enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };
                    if (!hasVision) enemyController.RunStart();

                    enemyController.StopCoroutine(attemptHideCoroutine);
                }

                yield return new WaitForSeconds(1f);
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

            if (!Methods.HasAimOnOpponent(out Character chara, enemyController, 60f)) {
                enemyController.enemy.RotateSelf(-enemyController.hidingLogic.currentCoverPosition.transform.position);
                List<CoverPose> poses = enemyController.hidingLogic.currentCoverPosition.GetCoverPoses();
                if (poses.Count == 1) {

                    if (poses[0] == CoverPose.Standing) {
                        enemyController.CrouchStart();
                    }
                }
                enemyController.aiState = AiState.BehindCover;
            } else {
                enemyController.aiState = AiState.Attacking;
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
        void MoveAgentAwayFromPoint(Vector3 avoidPoint) {
            // If the straight line approach fails, try other directions
            float angleIncrement = 30f; // degrees to increment
            for (float angle = 0f; angle < 360f; angle += angleIncrement) {
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