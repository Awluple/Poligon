namespace Poligon.EvetArgs {
    using System;

    public class InputValueEventArgs : EventArgs {
        public float Value { get; }

        public InputValueEventArgs(float value) {
            Value = value;
        }
    }
}