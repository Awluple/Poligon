using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Poligon.Ai.Commands {
    public class LastKnownPosition {
        public LastKnownPosition(Character ch, Vector3 pos) {
            position = pos;
            character = ch;
        }

        public Vector3 position;
        public Character character;
    }
}