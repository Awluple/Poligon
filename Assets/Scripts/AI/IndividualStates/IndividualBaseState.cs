using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai.States {

    public abstract class IndividualBaseState : State<IndividualAiState> {

        public override IndividualAiState state { get; protected set; }
        protected AiCharacterController aiController;

        public IndividualBaseState(AiCharacterController controller) {
            this.aiController = controller;
        }
    }
}

