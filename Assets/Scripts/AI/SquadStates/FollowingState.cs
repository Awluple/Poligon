using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Poligon.Ai.States {
    public class FollowingState : SquadBaseState {
        IEnumerator updatePositionCoroutine;
        IEnumerator trackLeaderPositionCoroutine;

        Vector3 lastCheckLeaderPosition;
        Vector3[] positions = new Vector3[5];
        public FollowingState(Squad squad) : base(squad) {
        }

        public override SquadAiState state { get; protected set; } = SquadAiState.FollowingState;


        public override void EnterState() {
            lastCheckLeaderPosition = -squad.leader.transform.forward;
            UpdatePositions();
            updatePositionCoroutine = UpdatePositionCoroutine();
            squad.StartCoroutine(updatePositionCoroutine);

            trackLeaderPositionCoroutine = TrackLeaderPositionChangeCoroutine();
            squad.StartCoroutine(trackLeaderPositionCoroutine);
        }

        public override void UpdateState() {

        }
        public override void ExitState() {
            squad.StopCoroutine(updatePositionCoroutine);
        }
        private Vector3 GetNavmeshPosition(AiCharacterController member, Vector3 desiredPosition) {
            NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 4f, NavMesh.AllAreas);
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(member.transform.position, hit.position, NavMesh.AllAreas, path);
            if (Vector3.Distance(path.corners[path.corners.Length - 1], desiredPosition) > 0.3f) {
                NavMesh.SamplePosition(path.corners[path.corners.Length - 1], out hit, 4f, NavMesh.AllAreas);
            }
            return hit.position;
        }
        IEnumerator TrackLeaderPositionChangeCoroutine() {
            Vector3 lastCheckLeaderBack = -squad.leader.transform.forward;        

            for (; ; ) {
                UpdatePositions();
                yield return new WaitForSeconds(1.4f);
            }
        }
        void UpdatePositions() {
            int[] degrees = { -60, -25, 0, 25, 60 };
            float[] distance = { 3f, 4f, 6f, 4f, 3f };
            Vector3 leaderBack = squad.leader.transform.forward;
            if (Vector3.Distance(squad.leader.transform.position, lastCheckLeaderPosition) > 4f) {
                for (int i = 0; i < degrees.Length; i++) {
                    Vector3 rotatedDirection = Quaternion.AngleAxis(degrees[i], Vector3.up) * leaderBack;
                    Vector3 finalPosition = squad.leader.transform.position - rotatedDirection.normalized * distance[i];
                    Debug.DrawLine(squad.leader.transform.position, finalPosition, Color.black, 1.4f);
                    lastCheckLeaderPosition = squad.leader.transform.position;
                    positions[i] = finalPosition;
                }
            }
        }
        IEnumerator UpdatePositionCoroutine() {
            for (; ; ) {
                var characters = squad.characters.Where(ch => ch.GetCharacter() != squad.leader).ToList();
                for (int i = 0; i < characters.Count; i++) {
                    AiCharacterController member = characters[i];
                    if (member.aiState != IndividualAiState.Patrolling) continue;
                    Vector3 position = GetNavmeshPosition(member, positions[i]);
                    if (member.aiState == IndividualAiState.Patrolling) {
                        member.SetNewDestinaction(position);
                    } else {
                        member.SetNewDestinaction(member.transform.position);
                    }
                    if(squad.leader.IsCrouching()) {
                        member.CrouchStart();
                        continue;
                    }
                    else
                    {
                        member.CrouchCancel();
                    }
                    if (Vector3.Distance(position, member.transform.position) > 10f) {
                        member.RunStart();
                    } else if (member.GetCharacter().IsRunning()) {
                        member.RunCancel();
                    }
                }
                yield return new WaitForSeconds(.2f);
            }
        }
        
    }
}