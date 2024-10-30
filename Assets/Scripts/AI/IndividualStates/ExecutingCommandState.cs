using System.Collections;
using UnityEngine;

namespace Poligon.Ai.States {
    public class ExecutingCommandState : IndividualBaseState {
        public ExecutingCommandState(AiCharacterController controller) : base(controller) {
        }

        public override IndividualAiState state { get; protected set; } = IndividualAiState.ExecutingCommand;


        public override void EnterState() {
        }
        public override void ExitState() {
        }

    }
}