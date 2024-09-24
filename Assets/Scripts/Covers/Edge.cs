using System.Collections.Generic;
using UnityEngine;

namespace Poligon {
    public class SubEdge {
        public Vector3 start;
        public Vector3 middle;
        public Vector3 end;
        public bool occupied;

        public SubEdge(Vector3 start, Vector3 end) {
            this.start = start;
            this.end = end;
            middle = GetMiddlePoint(start, end);
            occupied = false;
        }
        public static Vector3 GetMiddlePoint(Vector3 point1, Vector3 point2) {
            return point2 + (point1 - point2) / 2f;
        }
    }

    /// <summary>
    /// Holds information about NavMesh edge
    /// </summary>
    public class Edge {
        public int id;
        public Vector3 point1;
        public Vector3 point2;
        public Vector3 middle;
        public Vector3 forward;
        public float length;
        public int maxCapacity;
        public List<Character> currentOccupants;
        public List<SubEdge> subEdges;

        public Edge(Vector3 v1, Vector3 v2, int id) {
            if (v1.x < v2.x || (v1.x == v2.x && v1.z < v2.z)) {
                point1 = v1;
                point2 = v2;
            } else {
                point1 = v2;
                point2 = v1;
            }
            this.id = id;
            middle = GetMiddlePoint(v1, v2);
            length = Vector3.Distance(v1, v2);
            forward = Vector3.zero;
            maxCapacity = 0;
            currentOccupants = new();
            subEdges = new();
        }
        public static Vector3 GetMiddlePoint(Vector3 point1, Vector3 point2) {
            return point2 + (point1 - point2) / 2f;
        }
        public List<SubEdge> DivideEdge(Vector3 A, Vector3 B, float minLength) {
            List<SubEdge> subEdges = new List<SubEdge>();
            Vector3 direction = B - A;
            Vector3 unitDirection = direction.normalized;

            int subdivisions = Mathf.FloorToInt(length / minLength);
            float subLineLength = length / subdivisions;

            Vector3 currentPoint = A;
            for (int i = 0; i < subdivisions; i++) {
                Vector3 nextPoint = currentPoint + unitDirection * subLineLength;
                if (i == subdivisions - 1) nextPoint = B;

                subEdges.Add(new SubEdge(currentPoint, nextPoint));
                currentPoint = nextPoint;
            }

            return subEdges;
        }
        public override bool Equals(object obj) {
            if (!(obj is Edge))
                return false;

            Edge edge = (Edge)obj;
            return point1.Equals(edge.point1) && point2.Equals(edge.point2);
        }

        public override int GetHashCode() {
            return point1.GetHashCode() ^ point2.GetHashCode();
        }
    }
    public class EdgeBorderComparer : EqualityComparer<Collider> {
        private readonly int buildingsLayer;

        public EdgeBorderComparer() {
            buildingsLayer = LayerMask.NameToLayer("Buildings");
        }

        public override bool Equals(Collider x, Collider y) {
            if (ReferenceEquals(x.gameObject, y.gameObject)) {
                return true;
            }

            // Check if they have the same parent and that parent is on the "buildings" layer
            if (x != null && y != null &&
                x.gameObject.transform.parent != null && y.gameObject.transform.parent != null &&
                x.gameObject.transform.parent.gameObject.layer == buildingsLayer &&
                x.gameObject.transform.parent == y.gameObject.transform.parent) {
                return true;
            }
            return false;
        }
        public override int GetHashCode(Collider obj) {
            if (obj == null) {
                return 0;
            }

            int hash = obj.gameObject.GetHashCode();
            if (obj.transform.parent != null && obj.gameObject.transform.parent.gameObject.layer == buildingsLayer) {
                hash ^= obj.gameObject.transform.parent.GetHashCode();
            }

            return hash;
        }
    }
}