using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class EnemySpottedCommand : SquadCommand {
        private Character enemy;
        public EnemySpottedCommand(Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
        }

        public override void execute() {
            foreach(var character in squad.characters) {
                character.EnemySpotted(enemy);
            }
        }
    }
}