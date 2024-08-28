using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;


/// <summary>
/// Holds information about NavMesh edge
/// </summary>
public struct Edge {
    public Vector3 point1;
    public Vector3 point2;
    public float distance;

    public Edge(Vector3 v1, Vector3 v2) {
        if (v1.x < v2.x || (v1.x == v2.x && v1.z < v2.z)) {
            point1 = v1;
            point2 = v2;
        } else {
            point1 = v2;
            point2 = v1;
        }
        distance = Vector3.Distance(v1, v2);
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
/// <summary>
/// Gets NevMesh edges that surround buildings
/// </summary>
public class NavMeshBorderExtractor {
    List<Edge[]> triangles;
    public List<Vector3> GetPoints() {
        List<Vector3> borderPoints = GetNavMeshPoints(GetEdges());
        return borderPoints;
    }

    public List<Edge> GetEdges() {
        List<Edge> allEdges = new List<Edge>();
        if (triangles == null) {
            GetNavMeshTriangles();
        }
        return allEdges;
    }
    public static Vector3 GetMiddlePoint(Vector3 point1, Vector3 point2) {
        return point2 + (point1 - point2) / 2f;
    }
    /// <summary>
    /// Filter out edges that are not next to some building object
    /// </summary>
    /// <returns>List of edges next to buildings</returns>
    public List<Edge> FilterEdges() {
        if (triangles == null) {
            GetNavMeshTriangles();
        }
        int layerMask = (1 << 10);
        List<Edge> edges = new List<Edge>();
        foreach (var triangle in triangles) {
            for (int i = 0; i < triangle.Length; i++) {
                Collider[] hitColliders = Physics.OverlapBox(triangle[i].point1, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, layerMask);
                Collider[] hitColliders2 = Physics.OverlapBox(triangle[i].point2, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, layerMask);
                Vector3 middlePoint = GetMiddlePoint(triangle[i].point1, triangle[i].point2);
                middlePoint.y += 1f; // increase height in case that a building has floor 
                Collider[] hitColliders3 = Physics.OverlapBox(middlePoint, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, layerMask);
                if (hitColliders.Length > 0 && hitColliders2.Length > 0 && hitColliders3.Length > 0) {
                    EdgeBorderComparer comparer = new EdgeBorderComparer();
                    if (hitColliders.Intersect(hitColliders2, comparer).ToArray().Length > 0 ||
                        hitColliders2.Intersect(hitColliders3, comparer).ToArray().Length > 0) {
                        edges.Add(triangle[i]);
                    }
                }

            }
        }
        return edges;
    }
    /// <summary>
    /// Retrieves points from Edges
    /// </summary>
    /// <param name="edges">List of edges</param>
    /// <returns>List of NavMesh points</returns>
    public List<Vector3> GetNavMeshPoints(List<Edge> edges) {
        List<Vector3> borderPoints = new List<Vector3>();
        foreach (var kvp in edges) {
            borderPoints.Add(kvp.point1);
            borderPoints.Add(kvp.point2);
        }
        return borderPoints;
    }
    /// <summary>
    /// Encapsulates NavMesh points into list of triangles that build it
    /// </summary>
    /// <returns>List of NavMesh triangles</returns>
    public List<Edge[]> GetNavMeshTriangles() {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        List<Edge[]> triangles = new List<Edge[]>();
        for (int i = 0; i < navMeshData.indices.Length; i += 3) {
            int index1 = navMeshData.indices[i];
            int index2 = navMeshData.indices[i + 1];
            int index3 = navMeshData.indices[i + 2];

            Edge[] triangle = new Edge[]
            {
                new Edge(navMeshData.vertices[index1], navMeshData.vertices[index2]),
                new Edge(navMeshData.vertices[index2], navMeshData.vertices[index3]),
                new Edge(navMeshData.vertices[index3], navMeshData.vertices[index1])
            };
            triangles.Add(triangle);
        }

        this.triangles = triangles;
        return triangles;
    }
}

public class DynamicCover : MonoBehaviour {
    private NavMeshTriangulation navMeshData;
    private NavMeshBorderExtractor extractor;
    List<Edge> edges = new();

    void Start() {
        navMeshData = NavMesh.CalculateTriangulation();
        extractor = new NavMeshBorderExtractor();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (edges.Count == 0) {
                edges = extractor.FilterEdges();
            }
            ShowEdges();
        }
    }

    void ShowEdges() {
        foreach (var edge in edges) {
            var e1 = edge.point1;
            e1.y += 1f;
            var e2 = edge.point2;
            e2.y += 1f;
            Color color = edge.distance > .5f ? Color.red : Color.blue;
            Debug.DrawLine(e1, e2, color, 50f);
        }
    }
    //bool CheckEdge(Edge edge) {

    //    //Vector3 edgeDirection = (edge.vertex2 - edge.vertex1).normalized;

    //    Vector3 edgeMidpoint = GetMiddlePoint(edge.point1, edge.point2);
    //    Vector3 playerToEdge = (edgeMidpoint - player.position).normalized;

    //    //float angle = Vector3.Angle(playerToEdge, edgeDirection);

    //    // Check if the wall (edge) is facing away from the player (angle > 90)
    //    //if (angle > 90f) {
    //    //    // Perform a raycast to see if the player has line of sight to the edge

    //    //}
    //    if (Physics.Raycast(player.position, playerToEdge, out RaycastHit hit, Vector3.Distance(player.position, edgeMidpoint))) {
    //        edge.point1.y += 1f;
    //        edge.point2.y += 1f;
    //        Debug.DrawLine(edge.point1, edge.point2, Color.green, 100.0f); 
    //        return true;
    //    }
    //    return false;
    //}

    private void OnDrawGizmos() {

        //foreach (var cover in positions) {
        //    Vector3 direction = Vector3.Normalize(player.transform.position - cover);
        //    float distanceToPlayer = Vector3.Distance(player.transform.position, cover);
        //    if (distanceToPlayer > 8f) {
        //        Vector3 vectorTowardsPlayer = Vector3.RotateTowards(transform.forward, player.transform.position - transform.position, 999f, 999f);

        //        Vector3 directionToMe = Vector3.Normalize(transform.position - cover);
        //        float dotToMe = Vector3.Dot(cover, directionToMe);

        //        if (Vector3.Angle(vectorTowardsPlayer, cover - transform.position) < 60) {
        //            Gizmos.DrawCube(cover, new Vector3(0.2f, 0.2f, 0.2f));
        //        } else {
        //            Gizmos.DrawCube(cover, new Vector3(0.2f, 0.2f, 0.2f));
        //        }

        //    } else {
        //    }
        //}
    }
}