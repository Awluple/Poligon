namespace Poligon.EvetArgs {
using System;

    public class CoverPositionEventArgs : EventArgs {
        public CoverPosition coverPosition { get; }

        public CoverPositionEventArgs(CoverPosition value) {
            coverPosition = value;
        }
    }
}