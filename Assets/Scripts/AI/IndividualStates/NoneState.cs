using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Poligon.Ai.States {
    public enum SquadAiState {
        None,
        FollowingState,
    }

    public class SquadNoneState : SquadBaseState {
        public SquadNoneState(Squad squad) : base(squad) {
        }
        public override SquadAiState state { get; protected set; } = SquadAiState.None;
    }
}
