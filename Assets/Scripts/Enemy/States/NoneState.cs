using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Poligon.Ai.EnemyStates {
    public enum AiState {
        None,
        Patrolling,
        BehindCover,
        Hiding,
        Chasing,
        Attacking
    }

    public class NoneState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.None;
    }
}
