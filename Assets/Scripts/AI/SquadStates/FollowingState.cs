using System.Collections;
using UnityEngine;

namespace Poligon.Ai.States {
    public class FollowingState : SquadBaseState {
        IEnumerator updatePositionCoroutine;
        public FollowingState(Squad squad) : base(squad) {
        }

        public override SquadAiState state { get; protected set; } = SquadAiState.FollowingState;


        public override void EnterState() {
            updatePositionCoroutine = UpdatePositionCoroutine();
            squad.StartCoroutine(updatePositionCoroutine);
        }

        public override void UpdateState() {

        }
        public override void ExitState() {
            squad.StopCoroutine(updatePositionCoroutine);
        }

        IEnumerator UpdatePositionCoroutine() {
            for (; ; ) {
                foreach (AiCharacterController member in squad.characters) {
                    if(member.aiState != IndividualAiState.Patrolling) continue;
                    if (Vector3.Distance(squad.leader.transform.position, member.transform.position) > 5f && member.aiState == IndividualAiState.Patrolling) {
                        member.SetNewDestinaction(squad.leader.transform.position);
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
                    if (Vector3.Distance(squad.leader.transform.position, member.transform.position) > 10f) {
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