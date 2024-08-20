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
        StationaryAttacking,
        Searching
    }

    public class NoneState : EnemyBaseState {
        public NoneState(EnemyController controller) : base(controller) {
        }
        public override AiState state { get; protected set; } = AiState.None;
    }
}
