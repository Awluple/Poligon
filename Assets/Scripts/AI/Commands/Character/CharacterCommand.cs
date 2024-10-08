using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public abstract class CharacterCommand : ICommand {
        private ICharacterController character;
        CharacterCommand(ICharacterController character) {
            this.character = character;
        }
        public abstract void execute();
    }
}
