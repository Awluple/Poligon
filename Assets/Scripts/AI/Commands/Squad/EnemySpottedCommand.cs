using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.Ai.States;
using Poligon.Ai.States.Utils;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
namespace Poligon.Ai.Commands {
    public class EnemySpottedCommand : SquadCommand {
        private Character enemy;
        public EnemySpottedCommand(Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
        }

        public override void execute() {
            squad.UpdateLastKnownPosition(new LastKnownPosition(enemy, enemy.transform.position));
            if(squad.aiState == SquadAiState.FollowingState) squad.aiState = SquadAiState.EngagedState;
            NavMeshPath path = new NavMeshPath();
            foreach (var character in squad.characters) {
                if (character.attackingLogic.opponent == null) {
                    character.EnemySpotted(enemy);
                    continue;
                }
                character.navAgent.CalculatePath(character.aiCharacter.transform.position, path);
                if (squad.knownEnemies.Count == 1 && character.aiState != IndividualAiState.Chasing
                    && Methods.GetPathLength(path) > 20f && !Methods.HasVisionOnCharacter(out Character opChar, character, enemy)) {
                    character.attackingLogic.CallCharacter(enemy);
                }
            }
            squad.EnemySpotted(enemy);
        }
    }
}