using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public abstract class SquadCommand : ICommand {
        public Squad squad {
            get; private set;
        }
        public SquadCommand(Squad squad) {
            this.squad = squad;
        }

        public abstract void execute();
    }
}
