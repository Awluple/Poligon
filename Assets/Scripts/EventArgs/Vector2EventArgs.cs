using System;
using UnityEngine;

namespace Poligon.EvetArgs {
    public class Vector2EventArgs : EventArgs {
        public Vector2EventArgs(Vector2 vector) {
            Vector = vector;
        }
        public Vector2 Vector { get; set; }
    }
}