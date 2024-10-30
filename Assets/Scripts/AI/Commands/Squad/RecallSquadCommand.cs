using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
namespace Poligon.Ai.Commands {
    public class RecallSquadCommand : SquadCommand {
        public RecallSquadCommand(Squad squad) : base(squad) {
        }
        public override void execute() {
            foreach (AiCharacterController character in squad.characters) {
                character.aiState = States.IndividualAiState.ExecutingCommand;
                Vector3 position = squad.leader.transform.position;
                character.SetNewDestinaction(position);
                character.AimCancel();
                character.RunStart();
                character.OnFinalPositionEvent += (object sender, EventArgs args) => { SetState(character); };
            }
        }

        private void SetState(AiCharacterController character) {
            character.RunCancel();
            character.aiState = States.IndividualAiState.Patrolling;
        }
    }
}