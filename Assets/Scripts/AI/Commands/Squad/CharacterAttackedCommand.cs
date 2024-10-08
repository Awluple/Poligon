using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class CharacterAttackedCommand : SquadCommand {
        private Character enemy;
        private Character sender;
        public CharacterAttackedCommand(Character sender, Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
            this.sender = sender;
        }

        public override void execute() {
            squad.UpdateLastKnownPosition(new LastKnownPosition(enemy, enemy.transform.position));
            squad.EnemySpotted(enemy);
            foreach (var character in squad.characters) {
                if(character != sender && character.attackingLogic.opponent == null) character.EnemySpotted(enemy);
            }
        }
    }
}