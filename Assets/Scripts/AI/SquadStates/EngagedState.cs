using NUnit.Framework;
using Poligon.Ai.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Poligon.Ai.States {
    public class EngagedState : SquadBaseState {
        float noEnemiesTimeout = 0f;
        public EngagedState(Squad squad) : base(squad) {
        }

        public override SquadAiState state { get; protected set; } = SquadAiState.EngagedState;


        public override void EnterState() {
        }

        public override void UpdateState() {
            if (squad.knownEnemies.Count == 0) {
                noEnemiesTimeout += Time.deltaTime;
            } else {
                noEnemiesTimeout = 0f;
            }
            if(noEnemiesTimeout > 60f) {
                RecallSquadCommand recall = new(squad);
                recall.execute();
                squad.aiState = SquadAiState.FollowingState;
            }
            if(squad.gameObject.name == "Squad 2") {
                Debug.Log(squad.aiState);
            }
        }
        public override void ExitState() {
        }
    }
}