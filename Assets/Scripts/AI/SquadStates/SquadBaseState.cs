using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai.States {

    public abstract class SquadBaseState : State<SquadAiState> {

        public override SquadAiState state { get; protected set; }
        protected Squad squad;

        public SquadBaseState(Squad squad) {
            this.squad = squad;
        }
    }
}

