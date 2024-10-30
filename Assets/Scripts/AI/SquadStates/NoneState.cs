using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Poligon.Ai.States {
    public enum IndividualAiState {
        None,
        Patrolling,
        ExecutingCommand,
        BehindCover,
        Hiding,
        Chasing,
        StationaryAttacking,
        Searching
    }

    public class NoneState : IndividualBaseState {
        public NoneState(AiCharacterController controller) : base(controller) {
        }
        public override IndividualAiState state { get; protected set; } = IndividualAiState.None;
    }
}
