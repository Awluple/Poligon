using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class UpdateLastKnownPosCommand : SquadCommand {
        private Character enemy;
        public UpdateLastKnownPosCommand(Squad squad, Character enemy): base(squad) {
            this.enemy = enemy;
        }

        public override void execute() {
        }
    }
}