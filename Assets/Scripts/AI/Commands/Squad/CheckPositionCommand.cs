using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class CheckPositionCommand : SquadCommand {
        private Character enemy;
        public CheckPositionCommand(Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
        }

        public override void execute() {
            foreach(var character in squad.characters) {
                character.EnemySpotted(enemy);
            }
        }
    }
}