using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class CharacterAttacledCommand : SquadCommand {
        private Character enemy;
        private Character sender;
        public CharacterAttacledCommand(Character sender, Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
            this.sender = sender;
        }

        public override void execute() {
            squad.UpdateLastKnownPosition(new LastKnownPosition(enemy, enemy.transform.position));
            foreach(var character in squad.characters) {
                if(character != sender) character.EnemySpotted(enemy);
            }
        }
    }
}