using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai.EnemyStates {

    public abstract class EnemyBaseState : State<AiState> {

        public override AiState state { get; protected set; }
        protected EnemyController enemyController;

        public EnemyBaseState(EnemyController controller) {
            this.enemyController = controller;
        }
    }
}

