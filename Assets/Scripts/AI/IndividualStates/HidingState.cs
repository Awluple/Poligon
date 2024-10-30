using System.Collections;
using UnityEngine;
using Poligon.Ai.States.Utils;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

namespace Poligon.Ai.States {
    public class HidingState : IndividualBaseState {
        private IEnumerator movingAttackCoroutine;
        private IEnumerator attemptHideCoroutine;
        float timeSinceLastSeen = Time.time;
        float maxNavMeshSampleDistance = 5f;
        float maxBackOffDistance = 7f;
        private SubEdge? subEdge = null;
        bool coverSearchingStarted = false;

        public HidingState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.Hiding;

        public override void EnterState() {
            if (aiController.aiCharacter.destroyCancellationToken.IsCancellationRequested) return;
            aiController.SetNewDestinaction(aiController.aiCharacter.transform.position);
            if (aiController.hidingLogic.currentCoverSubEdge != null) {
                aiController.SetNewDestinaction(aiController.hidingLogic.currentCoverSubEdge.middle);
                aiController.OnFinalPositionEvent += OnPosition;
                aiController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { aiController.RunCancel(); };
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(aiController, true, 0.15f);

            } else {
                if (Vector3.Distance(aiController.transform.position, aiController.attackingLogic.opponent.transform.position) > 7f) {
                    aiController.SetNewDestinaction(aiController.aiCharacter.transform.position);
                    UnityEngine.Random.InitState(aiController.transform.position.GetHashCode());
                    var number = UnityEngine.Random.Range(0f, 10f);
                    if (number > 7f) aiController.CrouchStart();
                    aiController.aiCharacter.GetAimPosition().LockOnTarget(aiController.attackingLogic.opponent, !aiController.aiCharacter.IsAiming());
                } else {
                    MoveAgentAwayFromPoint(aiController.attackingLogic.opponent.transform.position);
                }
                attemptHideCoroutine = AttemptHideCoroutine();
                aiController.StartCoroutine(attemptHideCoroutine);
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(aiController, false);
            }


            aiController.StartCoroutine(movingAttackCoroutine);
        }

        private IEnumerator AttemptHideCoroutine() {
            for (; ; ) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, aiController);
                if (hasVision) {
                    if (!coverSearchingStarted) aiController.hidingLogic.GetHidingSubEdge(aiController.attackingLogic.opponent.transform.position, aiController.transform.position);
                    coverSearchingStarted = true;
                    timeSinceLastSeen = Time.time;
                } else {
                    if (Time.time - timeSinceLastSeen > 5.9f && aiController.attackingLogic.opponent != null) {
                        if(!coverSearchingStarted) aiController.hidingLogic.GetHidingSubEdge(aiController.attackingLogic.opponent.transform.position, aiController.transform.position);
                        coverSearchingStarted = true;
                    }
                }
                if (aiController.hidingLogic.currentCoverEdge != null && aiController.hidingLogic.currentCoverSubEdge != null) {
                    bool claimed = aiController.hidingLogic.dynamicCover.Subscribe(aiController.hidingLogic.currentCoverEdge.id, aiController.hidingLogic.currentCoverSubEdge, aiController.aiCharacter);
                    
                    if(NavMesh.SamplePosition(aiController.hidingLogic.currentCoverSubEdge.middle, out NavMeshHit hit, 1f, NavMesh.AllAreas)) {
                        if (!claimed) {
                            aiController.hidingLogic.ClearDynamicCover();
                            coverSearchingStarted = false;
                        } else {
                            aiController.CrouchCancel();
                            aiController.SetNewDestinaction(hit.position);
                            if (!hasVision) aiController.RunStart();
                            Debug.DrawLine(aiController.hidingLogic.currentCoverSubEdge.start, aiController.hidingLogic.currentCoverSubEdge.end, Color.green, 10f);
                            aiController.StopCoroutine(attemptHideCoroutine);
                            aiController.OnFinalPositionEvent += OnPosition;
                            aiController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { aiController.RunCancel(); };
                        }  
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private void OnPosition(object sender = null, System.EventArgs e = null) {
            aiController.OnFinalPositionEvent -= OnPosition;
            SetBehindCoverPosition();
        }
        public override void ExitState() {
            if (movingAttackCoroutine != null) aiController.StopCoroutine(movingAttackCoroutine);
            if (attemptHideCoroutine != null) aiController.StopCoroutine(attemptHideCoroutine);

            aiController.OnFinalPositionEvent -= OnPosition;
            aiController.OnFinalPositionEvent -= (object sender, System.EventArgs e) => { aiController.RunCancel(); };

            aiController.ShootCancel();
        }
        public void SetBehindCoverPosition() {
            if (aiController.aiState != state) return;
            if (!Methods.HasAimOnOpponent(out Character chara, aiController, 60f)) {
                try {
                    aiController.aiCharacter.RotateSelf(-aiController.hidingLogic.currentCoverEdge.forward);
                } catch (Exception e) {
                    Debug.LogError("Cover Edge: " + aiController.hidingLogic.currentCoverEdge);
                    throw;
                }
                aiController.CrouchStart();
                aiController.aiState = IndividualAiState.BehindCover;
            } else {
                aiController.aiState = IndividualAiState.StationaryAttacking;
            }
        }
        bool IsPathWithinDistance(Vector3 targetPosition, float maxDistance) {
            NavMeshPath path = new NavMeshPath();
            aiController.navAgent.CalculatePath(targetPosition, path);
            float pathLength = Methods.GetPathLength(path);
            return pathLength <= maxDistance;
        }
        
        void MoveAgentAwayFromPoint(Vector3 avoidPoint) {
            // If the straight line approach fails, try other directions
            float angleIncrement = 30f; // degrees to increment
            for (float angle = 0f; angle < 360f; angle += angleIncrement) {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * (aiController.navAgent.transform.position - avoidPoint).normalized;
                Vector3 targetPosition = aiController.navAgent.transform.position + direction * maxBackOffDistance;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, maxNavMeshSampleDistance, NavMesh.AllAreas)) {
                    if (IsPathWithinDistance(hit.position, maxBackOffDistance)) {
                        aiController.SetNewDestinaction(hit.position);
                        break;
                    }
                }
            }
        }
    }
}