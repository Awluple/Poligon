namespace Poligon.EvetArgs {
    using System;

    public class DeathEventArgs : EventArgs {
        public Character character { get; }

        public DeathEventArgs(Character value) {
            character = value;
        }
    }
}