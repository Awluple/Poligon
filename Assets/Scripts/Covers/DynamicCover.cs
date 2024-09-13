using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Build;
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
    public int id;
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 middle;
    public Vector3 forward;
    public float distance;
    public int maxCapacity;
    public List<Character> currentOccupants;

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
        distance = Vector3.Distance(v1, v2);
        forward = Vector3.zero;
        maxCapacity = 0;
        currentOccupants = new();
    }
    public static Vector3 GetMiddlePoint(Vector3 point1, Vector3 point2) {
        return point2 + (point1 - point2) / 2f;
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
    
    /// <summary>
    /// Filter out edges that are not next to some building object
    /// </summary>
    /// <returns>List of edges next to buildings</returns>
    public Dictionary<int,Edge> FilterEdges() {

        var buildingsLayer = LayerMask.NameToLayer("Buildings");


        if (triangles == null) {
            GetNavMeshTriangles();
        }
        int buildingsLayerMask = (1 << 10);
        int groundLayerMask = (1 << 6) | (1 << 10);
        Dictionary<int, Edge> edges = new Dictionary<int, Edge>();
        foreach (var triangle in triangles) {
            for (int i = 0; i < triangle.Length; i++) {
                Collider[] hitColliders = Physics.OverlapBox(triangle[i].point1, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, buildingsLayerMask);
                Collider[] hitColliders2 = Physics.OverlapBox(triangle[i].point2, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, buildingsLayerMask);
                Vector3 middlePoint = triangle[i].middle;
                middlePoint.y += 1f; // increase height in case that a building has floor 
                Collider[] hitColliders3 = Physics.OverlapBox(middlePoint, new Vector3(1.2f, 1.2f, 1.2f) / 2, Quaternion.identity, buildingsLayerMask);
                if (hitColliders.Length > 0 && hitColliders2.Length > 0 && hitColliders3.Length > 0) {
                    EdgeBorderComparer comparer = new EdgeBorderComparer();
                    if (hitColliders.Intersect(hitColliders2, comparer).ToArray().Length > 0 ||
                        hitColliders2.Intersect(hitColliders3, comparer).ToArray().Length > 0) {
                        middlePoint.y -= 1f;


                        if (!Physics.Raycast(middlePoint, Vector3.down, 1.2f, groundLayerMask)) {// Remove flying edges
                            continue;
                        }
                        //attempt to find the forward direction of an edge
                        float extraHeight = 0f;
                        float maxDistance = 0.7f;
                        while (triangle[i].forward == Vector3.zero && extraHeight < 1.2f) {
                            Vector3 startPoint = triangle[i].point1;
                            startPoint.y += extraHeight;
                            middlePoint.y += extraHeight;
                            triangle[i].forward = GetForwardDirectionNormalized(startPoint, middlePoint, maxDistance, buildingsLayerMask);
                            extraHeight += 0.2f;
                            maxDistance += 0.1f;
                        }
                        if (triangle[i].forward != Vector3.zero) {
                            triangle[i].maxCapacity = (int)Mathf.Floor(triangle[i].distance / 1.2f);
                            edges.Add(triangle[i].id, triangle[i]);
                        } 
                        
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
                new Edge(navMeshData.vertices[index1], navMeshData.vertices[index2], i-2),
                new Edge(navMeshData.vertices[index2], navMeshData.vertices[index3], i-1),
                new Edge(navMeshData.vertices[index3], navMeshData.vertices[index1], i)

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
    Dictionary<int, Edge> edges = new();

    void Start() {
        navMeshData = NavMesh.CalculateTriangulation();
        extractor = new NavMeshBorderExtractor();
        edges = extractor.FilterEdges();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //if (edges.Count == 0) {
            //    edges = extractor.FilterEdges();
            //}
            //ShowEdges();
        }
    }
    /// <summary>
    /// Add a character to a cover edge
    /// </summary>
    /// <param name="id">Id of an  edge</param>
    /// <param name="character">Character to add</param>
    /// <returns>True if character is successfully added, otherwise false</returns>
    public bool Subscribe(int id, Character character) {
        Edge edge = edges[id];
        if(edge.currentOccupants.Count >= edge.maxCapacity) return false;
        edge.currentOccupants.Add(character);
        return true;
    }
    /// <summary>
    /// Remove a character from a cover edge
    /// </summary>
    /// <param name="id">Id of an  edge</param>
    /// <param name="character">Character to remove</param>
    /// <returns>true if character is successfully removed; otherwise, false. This method also returns false if item was not found in the </returns>
    public bool UnSubscribe(int id, Character character) {
        Edge edge = edges[id];
        return edge.currentOccupants.Remove(character);
    }
    /// <summary>
    /// Shows edges for debug
    /// </summary>
    void ShowEdges() {
        foreach (var edge in edges.Values) {
            var e1 = edge.point1;
            e1.y += 1f;
            var e2 = edge.point2;
            e2.y += 1f;
            Color color = edge.distance > .5f ? Color.red : Color.blue;
            Debug.DrawLine(e1, e2, color, 50f);
            Debug.DrawRay(edge.middle, edge.forward, Color.green, 100f);
        }
    }
    /// <summary>
    /// Get covers close to a position.
    /// </summary>
    /// <param name="position">Position where to seek close covers</param>
    /// <param name="agent">Navmesh agent for path calculation</param>
    /// <param name="absoluteDistance">Maximum distance to covers in straight line</param>
    /// <param name="pathingDistance">Maximum distance to covers that navmesh agent must travel</param>
    /// <returns></returns>
    public async Awaitable<List<Edge>> GetCovers(Vector3 position, NavMeshAgent agent, float absoluteDistance = 25f, float pathingDistance = 20f) {
        var absolute = await GetAboluteCloseCovers(position, absoluteDistance);
        var navmesh = GetNavmeshCloseCovers(absolute, agent, position, pathingDistance);
        return navmesh;
    }

    public List<Edge> GetValidHidingPositions(Vector3 hidingSourcePosition, Character character, List<Edge> edges, Vector3? originPosition = null,
        bool closestCover = true, bool ignoreDot = false, float minDistanceFromSource = 8f, float maxCoverPathDistance = 20f, float maxSearchDistance = 21f) {
        // Get the starting point, if not provided use transform.position
        List<Edge> validEdges = new List<Edge>();
        Vector3 originPos = Vector3.zero;
        if (originPosition == null) originPos = transform.position;
        else { originPos = originPosition.GetValueOrDefault(); }

        // Sort covers by distance from origin position
        if (closestCover) {
            Array.Sort(edges.ToArray(), (a, b) => Vector3.Distance(a.middle, originPos).CompareTo(Vector3.Distance(b.middle, originPos)));
        } else {
            Array.Sort(edges.ToArray(), (a, b) => Vector3.Distance(b.middle, originPos).CompareTo(Vector3.Distance(a.middle, originPos)));
        }
        foreach (Edge edge in edges) {
            if (edge.maxCapacity < 1) continue;

            Vector3 direction = Vector3.Normalize(hidingSourcePosition - edge.middle);
            float dotToTarget = Vector3.Dot(edge.forward, direction);

            if (Vector3.Distance(hidingSourcePosition, edge.middle) > minDistanceFromSource && dotToTarget > 0.3f) {

                Vector3 vectorTowardsSource = Vector3.RotateTowards(edge.forward, hidingSourcePosition - originPosition.GetValueOrDefault(), 999f, 999f);
                Vector3 directionToMe = Vector3.Normalize(originPosition.GetValueOrDefault() - edge.middle);
                float dotToMe = Vector3.Dot(edge.forward, directionToMe);

                if (!ignoreDot && Vector3.Angle(vectorTowardsSource, edge.middle - originPosition.GetValueOrDefault()) < 60) {
                    if (!(dotToTarget > 0.6f && dotToMe < 0.25f)) {
                        continue;
                    }
                }
                validEdges.Add(edge);
            }
        }

        return validEdges;
    }
    /// <summary>
    /// Scores cover edges and selects the best one
    /// </summary>
    /// <param name="hidingSourcePosition">The location of object from which to hide</param>
    /// <param name="edges">List of edges to check</param>
    /// <param name="agent">Navmesh agent for path calculation</param>
    /// <returns>The best cover edge or null if nothing found</returns>
    public Edge? GetBestPosition(Vector3 hidingSourcePosition, List<Edge> edges, NavMeshAgent agent) {
        NavMeshPath path = new NavMeshPath();
        List<(float, Edge)> positions = new();
        foreach (Edge edge in edges) {
            float points = 0;
            agent.CalculatePath(edge.middle, path);
            float distance = CalculatePath(path.corners);
            points += 85 - ((distance * 10) / 3);
            if (Vector3.Distance(hidingSourcePosition, edge.middle) < 10f) {
                points -= 35;
            }
            int layerMask = (1 << 9) | (1 << 20) | (1 << 8) | (1 << 11);
            // Check shooting positions
            Vector3 position = edge.middle;
            position.y += 1.8f;
            bool hasVisionMiddle = Physics.Raycast(position, edge.forward, 7f, layerMask);
            position = edge.point1;
            position.y += 1.8f;
            bool hasVision1 = Physics.Raycast(position, edge.forward, 7f, layerMask);
            position = edge.point2;
            position.y += 1.8f;
            bool hasVision2 = Physics.Raycast(position, edge.forward, 7f, layerMask);

            if (hasVisionMiddle || hasVision1 || hasVision2) {
                points += 50;
            }else { // There are no easy (standing) shooting positions, check leaning from corners instead
                position = edge.point1;
                Vector3 direction = (edge.middle - edge.point1).normalized;
                position = edge.point1 + direction * 1.5f;
                position.y += 1.8f;
                hasVision1 = Physics.Raycast(position, edge.forward, 7f, layerMask);

                position = edge.point2;
                direction = (edge.middle - edge.point2).normalized;
                position = edge.point2 + direction * 1.5f;
                position.y += 1.8f;
                hasVision2 = Physics.Raycast(position, edge.forward, 7f, layerMask);
                if (hasVision1 || hasVision2) {
                    points += 20;
                }
            }

            // Check dot value to see if cover has good angle
            Vector3 dir = Vector3.Normalize(hidingSourcePosition - edge.middle);
            float dotToTarget = Vector3.Dot(edge.forward, dir);
            if (dotToTarget > 0.75f) {
                points += 50;
            }
            positions.Add((points, edge));
        }
        positions.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        if (positions.Count > 0) { 
            return positions.Select(t => t.Item2).Last();
        }
        return null;
    }
    /// <summary>
    /// Get close edge covers in straight line
    /// </summary>
    /// <param name="position">The starting position</param>
    /// <param name="distance">Maximum distance</param>
    /// <returns>List of clost edges</returns>
    private async Awaitable<List<Edge>> GetAboluteCloseCovers(Vector3 position, float distance) {
        List<Edge> filteredEdges = await Task.Run(() => {
            return edges.Where(i => Vector3.Distance(position, i.Value.middle) < distance).Select(s => s.Value).ToList();
        });
        return filteredEdges;
    }
    /// <summary>
    /// Calculate path from navmesh
    /// </summary>
    /// <param name="corners">Corners of path</param>
    /// <returns>Distance to travel</returns>
    private float CalculatePath(Vector3[] corners) {
        float totalDistance = 0.0f;

        for (int i = 0; i < corners.Length - 1; i++) {
            totalDistance += Vector3.Distance(corners[i], corners[i + 1]);
        }
        return totalDistance;
    }
    /// <summary>
    /// Get close edge covers by distance that navmesh agent needs to travel
    /// </summary>
    /// <param name="edges">List of close edges</param>
    /// <param name="agent">Navmesh agent</param>
    /// <param name="position">Starting position</param>
    /// <param name="distance">Maximum travel distance</param>
    /// <returns>List of clost edges</returns>
    private List<Edge> GetNavmeshCloseCovers(List<Edge> edges, NavMeshAgent agent, Vector3 position, float distance) {

        List<Edge> filteredEdges = new();
        var path = new NavMeshPath();
            try {
            foreach (var edge in edges) {
                var hasPath = false;
                hasPath = agent.CalculatePath(edge.middle, path);
                if (!hasPath) continue;
                float totalPath = CalculatePath(path.corners);
                if (totalPath < distance) filteredEdges.Add(edge);
            }
            } catch (Exception e) {
            Debug.LogError(e);
            throw;
            }
        return filteredEdges;
    }
}