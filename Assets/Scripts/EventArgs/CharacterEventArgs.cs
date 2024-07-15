namespace Poligon.EvetArgs {
    using System;

    public class CharacterEventArgs : EventArgs {
        public Character character { get; }

        public CharacterEventArgs(Character value) {
            character = value;
        }
    }
}