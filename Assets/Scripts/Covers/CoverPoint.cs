using UnityEngine;


namespace Poligon {

    public enum CoverPointAxis {
        Y,
        X,
    }
    public enum CoverEdgePointSide {
        Left,
        Right,
        Both,
        None
    }

    public struct CoverPoint {
        public CoverPoint(Vector3 pos, CoverPointAxis coverAxis, bool coverIsEdgeCover, CoverEdgePointSide side = CoverEdgePointSide.None) {
            position = pos;
            axis = coverAxis;
            isEdgeCover = coverIsEdgeCover;
            edgePointSide = side;
        }

        public Vector3 position;
        public CoverPointAxis axis;
        public bool isEdgeCover;
        public CoverEdgePointSide edgePointSide;


        public override string ToString() {
            return "Position: " + position + " | Axis: " + axis + " | Is edge cover: " + isEdgeCover;
        }
    }
}