using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UI.Image;

/// <summary>
/// Holds information about NavMesh edge
/// </summary>
public struct Edge {
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 forward;
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
        forward = Vector3.zero;
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

        var buildingsLayer = LayerMask.NameToLayer("Buildings");


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
                        middlePoint.y -= 1f;

                        //attempt to find the forward direction of an edge

                        float extraHeight = 0f;
                        float maxDistance = 0.7f;
                        while (triangle[i].forward == Vector3.zero && extraHeight < 1.6f) {
                            Vector3 startPoint = triangle[i].point1;
                            startPoint.y += extraHeight;
                            middlePoint.y += extraHeight;
                            triangle[i].forward = GetForwardDirectionNormalized(startPoint, middlePoint, maxDistance, layerMask);
                            extraHeight += 0.2f;
                            maxDistance += 0.1f;
                        }
                        if(triangle[i].forward != Vector3.zero) edges.Add(triangle[i]);

                    }
                }

            }
        }
        return edges;
    }
    /// <summary>
    /// Get normalized direction of 90 degree relative to start point and end point where RayCast hit object on layerMask
    /// </summary>
    /// <param name="start">The start of a line</param>
    /// <param name="end">The end of a line</param>
    /// <param name="distance">The distance for raycast</param>
    /// <param name="layerMask">Layers to seek collision</param>
    /// <returns>Normalized Vector3 direction or Vector3.zero if no raycast hits</returns>
    public Vector3 GetForwardDirectionNormalized(Vector3 start, Vector3 end, float distance, LayerMask layerMask) {
        var (directionA, directionB) = GetPerpendicularDirectionsNormalized(start, end);
        var rayA = Physics.Raycast(new Ray(end, directionA), out RaycastHit hitA, distance, layerMask);
        var rayB = Physics.Raycast(new Ray(end, directionB), out RaycastHit hitB, distance, layerMask);

        if (rayA && rayB) {
            if (Vector3.Distance(hitA.point, end) < Vector3.Distance(hitB.point, end)) {
                return directionB;
            } else {
                return directionA;
            }
        } else if (rayA) {
            return directionA;
        } else if (rayB) {
            return directionB;
        }
        return Vector3.zero;
    }
    /// <summary>
    /// Get two normalised directions of 90 degree relative to pointA and pointB 
    /// </summary>
    /// <param name="pointA">The start of a line</param>
    /// <param name="pointB">The end of a line</param>
    /// <returns>Two normalised Vector3 directions</returns>
    public (Vector3, Vector3) GetPerpendicularDirectionsNormalized(Vector3 pointA, Vector3 pointB) {
        // Calculate direction vector from A to B
        Vector3 direction = (pointB - pointA).normalized;

        // Create a perpendicular vector
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        // If the result is zero (direction is parallel to Vector3.up), use Vector3.forward instead
        if (perpendicular == Vector3.zero) {
            perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        }

        // Return both perpendicular directions
        return (perpendicular, -perpendicular);
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
    private Character character;
    private NavMeshAgent agent;
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
            Debug.DrawRay(NavMeshBorderExtractor.GetMiddlePoint(e1, e2), edge.forward, Color.green, 100f);
        }
    }
    bool isCloseCover(Vector3 position, float maxCoverDistance, out NavMeshPath path) {
        path = new NavMeshPath();

        if (!agent.CalculatePath(position, path)) {
            return false;
        }
        float distance = 0;
        for (int i = 0; i < path.corners.Length; i++) {
            if (i + 1 > path.corners.Length - 1) break;
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return distance < maxCoverDistance;
    }
    //void CheckEdge(List<Edge> edges, Vector3 hidingSourcePosition, Character character, Vector3? originPosition = null,
    //    bool closestCover = true, bool ignoreDot = false, float minDistanceFromSource = 8f, float maxCoverPathDistance = 20f, float maxSearchDistance = 21f) {
    //    var sorted = edges.OrderBy(e => (e.point1 - character.transform.position).sqrMagnitude).ToList();
    //    Vector3 originPos = Vector3.zero;
    //    if (originPosition == null) originPos = transform.position;
    //    else { originPos = originPosition.GetValueOrDefault(); }
        
    //    foreach (var edge in sorted) {
    //        Vector3 middlePoint = NavMeshBorderExtractor.GetMiddlePoint(edge.point1, edge.point2);
    //        if (Vector3.Distance(originPos, middlePoint) > maxSearchDistance) { continue; };


    //        Vector3 direction = Vector3.Normalize(hidingSourcePosition - middlePoint);
    //        //float dotToTarget = Vector3.Dot(middlePoint.transform.forward, direction);

    //        if (Vector3.Distance(hidingSourcePosition, middlePoint) > minDistanceFromSource &&  isCloseCover(middlePoint, maxCoverPathDistance, out NavMeshPath path)) {

    //            //Vector3 towards = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);
    //            //Debug.DrawRay(transform.position, towards * 10, Color.red, 40f);

    //            Vector3 vectorTowardsSource = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);

    //            Vector3 directionToMe = Vector3.Normalize(transform.position - middlePoint);
    //            //float dotToMe = Vector3.Dot(cover.transform.forward, directionToMe);

    //            if (!ignoreDot && Vector3.Angle(vectorTowardsSource, cover.transform.position - transform.position) < 60) {
    //                if (!(dotToTarget > 0.6f && dotToMe < 0.25f)) {
    //                    continue;
    //                }
    //            }
    //            // Draw path for debug
    //            pathLine.positionCount = path.corners.Length;
    //            pathLine.SetPosition(0, transform.position);
    //            for (int i = 1; i < path.corners.Length; i++) {
    //                pathLine.SetPosition(i, path.corners[i]);
    //            }

    //            if (currentCoverPosition != null) currentCoverPosition.occuped = false;
    //            currentCoverPosition = coverPosition;
    //            currentCoverPosition.occuped = true;
    //            currentCoverPosition.occupedBy = character;
    //            achievedPosition = false;
    //            return cover.transform.position;
    //        }
    //    }

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