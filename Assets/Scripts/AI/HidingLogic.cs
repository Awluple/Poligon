using Poligon.EvetArgs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Poligon;
using Poligon.Ai;
using System.Collections.Generic;
/// <summary>
/// Includes logic for finding cover positions
/// </summary>
public class HidingLogic : MonoBehaviour {
    public Character character;
    private Player player; // TEMP used only for debug in onDrawGizmos

    private NavMeshAgent agent;
    private List<Edge> edges;
    private bool ready = false;
    [SerializeField] private LayerMask hidableLayers;

    [SerializeField] HidingCollisionSphere hidingSphere;

    //private Mesh mesh;

    private LineRenderer pathLine;

    public CoverPosition currentCoverPosition { get; private set; }
    public DynamicCover dynamicCover;
    public Edge? currentCoverEdge { get; private set; }
    public SubEdge? currentCoverSubEdge { get; private set; }

    private bool achievedPosition = false;

    private void Update() {
        //if (currentCoverPosition && achievedPosition && Vector3.Distance(character.transform.position, currentCoverPosition.transform.position) > 1f) {
        //    currentCoverPosition.occuped = false;
        //    currentCoverPosition = null;
        //} else if (currentCoverPosition && Vector3.Distance(character.transform.position, currentCoverPosition.transform.position) < 1f) {
        //    achievedPosition = true;
        //}
        if (currentCoverEdge != null && achievedPosition && Vector3.Distance(character.transform.position, currentCoverSubEdge.middle) > 1f) {
            if (currentCoverEdge != null) dynamicCover.UnSubscribe(currentCoverEdge.id, currentCoverSubEdge, character);
            currentCoverEdge = null;
            currentCoverSubEdge = null;
        } else if (currentCoverEdge != null && Vector3.Distance(character.transform.position, currentCoverSubEdge.middle) < 0.4f) {
            achievedPosition = true;
        }
    }

    private void Awake() {
        agent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        player = FindFirstObjectByType<Player>();
        dynamicCover = FindFirstObjectByType<DynamicCover>();


        character = GetComponentInParent<Character>();
        hidingSphere = character.GetComponentInChildren<HidingCollisionSphere>();
        character.OnDeath += (object sender, CharacterEventArgs args) => {
            //if (currentCoverPosition != null) currentCoverPosition.occuped = false; 
            if (currentCoverSubEdge != null) dynamicCover.UnSubscribe(currentCoverEdge.id, currentCoverSubEdge, character);
        };

    }

    private void Start() {
        pathLine = gameObject.AddComponent<LineRenderer>();
        pathLine.startWidth = 0.2f;
        pathLine.endWidth = 0.2f;
        pathLine.positionCount = 0;
    }

    public void ClearDynamicCover() {
        currentCoverSubEdge = null;
        currentCoverEdge = null;
    }

    /// <summary>
    /// Calculates path to the cover position
    /// </summary>
    /// <param name="position">Starting position</param>
    /// <param name="maxCoverDistance">Max distance</param>
    /// <param name="path">Path object</param>
    /// <returns>True if distance is lower than maxCoverDistance</returns>
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
    /// <summary>
    /// Get the cover position
    /// </summary>
    /// <param name="hidingSourcePosition">The source to hide from</param>
    /// <param name="character">Character that wants to hide</param>
    /// <param name="originPosition">The position of object that is wants to hide</param>
    /// <param name="closestCover">If true returns the closest cover to originPos, if false - the farthest</param>
    /// <param name="ignoreDot">If true calculate cover position in ralation to the target to return ones that are on target's side, if false return any cover that meets distance requirements</param>
    /// <param name="minDistanceFromSource">The minimum distance the cover must be from the target</param>
    /// <param name="maxCoverPathDistance">Maximum distanse to travel</param>
    /// <param name="maxSearchDistance">Maximum distance to search for cover</param>
    /// <returns>Cover position</returns>
    public async Awaitable<SubEdge?> GetHidingSubEdge(Vector3 hidingSourcePosition, Vector3? originPosition = null,
        bool closestCover = true, bool ignoreDot = false, float minDistanceFromSource = 8f, float maxCoverPathDistance = 20f, float maxSearchDistance = 21f) {
        try {
            edges = await dynamicCover.GetCovers(transform.position, agent, maxSearchDistance, maxCoverPathDistance);
        } catch (Exception e) { 
            Debug.LogError(e);
            throw;
        }
        var positions = dynamicCover.GetValidHidingPositions(hidingSourcePosition, edges, originPosition, closestCover, ignoreDot, minDistanceFromSource);
        if (positions.Count > 0) {
            var bestPosition = dynamicCover.GetBestPosition(hidingSourcePosition, positions, agent);
            List<(float, SubEdge)> distances = new();
            NavMeshPath path = new NavMeshPath();
            foreach (var subEdge in bestPosition.subEdges) {
                agent.CalculatePath(subEdge.middle, path);
                distances.Add((dynamicCover.CalculatePath(path.corners), subEdge));
            }
            var hidingSubEdge = distances.Where(d => !d.Item2.occupied).OrderBy(s => s.Item1).Select(d => d.Item2).First();
            currentCoverEdge = bestPosition;
            currentCoverSubEdge = hidingSubEdge;
            return hidingSubEdge;
            //Debug.DrawLine(coverPosition.start, coverPosition.end, Color.green, 10f);

        } else {
            return null;
        }
    }

    /// <summary>
    /// Get the cover position
    /// </summary>
    /// <param name="hidingSourcePosition">The source to hide from</param>
    /// <param name="character">Character that wants to hide</param>
    /// <param name="originPosition">The position of object that is wants to hide</param>
    /// <param name="closestCover">If true returns the closest cover to originPos, if false - the farthest</param>
    /// <param name="ignoreDot">If true calculate cover position in ralation to the target to return ones that are on target's side, if false return any cover that meets distance requirements</param>
    /// <param name="minDistanceFromSource">The minimum distance the cover must be from the target</param>
    /// <param name="maxCoverPathDistance">Maximum distanse to travel</param>
    /// <param name="maxSearchDistance">Maximum distance to search for cover</param>
    /// <returns>Cover position</returns>
    //public Vector3 GetHidingPosition(Vector3 hidingSourcePosition, Character character, Vector3? originPosition = null,
    //    bool closestCover = true, bool ignoreDot = false, float minDistanceFromSource = 8f, float maxCoverPathDistance = 20f, float maxSearchDistance = 21f) {

    //    // Get the starting point, if not provided use transform.position
    //    Vector3 originPos = Vector3.zero;
    //    if (originPosition == null) originPos = transform.position;
    //    else { originPos = originPosition.GetValueOrDefault(); }

    //    // Sort covers by distance from origin position
    //    GameObject[] values = hidingSphere.GetCovers().Values.ToArray();
    //    if (closestCover) {
    //        Array.Sort(values, (a, b) => Vector3.Distance(a.transform.position, originPos).CompareTo(Vector3.Distance(b.transform.position, originPos)));
    //    } else {
    //        Array.Sort(values, (a, b) => Vector3.Distance(b.transform.position, originPos).CompareTo(Vector3.Distance(a.transform.position, originPos)));
    //    }
    //    foreach (GameObject cover in values) {
    //        if (Vector3.Distance(originPos, cover.transform.position) > maxSearchDistance) { continue; };

    //        CoverPosition coverPosition = cover.GetComponent<CoverPosition>();
    //        if (coverPosition.occuped && coverPosition.occupedBy != character) continue;

    //        Vector3 direction = Vector3.Normalize(hidingSourcePosition - cover.transform.position);
    //        float dotToTarget = Vector3.Dot(cover.transform.forward, direction);

    //        if (Vector3.Distance(hidingSourcePosition, cover.transform.position) > minDistanceFromSource && dotToTarget > 0.3f && isCloseCover(cover.transform.position, maxCoverPathDistance, out NavMeshPath path)) {

    //            //Vector3 towards = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);
    //            //Debug.DrawRay(transform.position, towards * 10, Color.red, 40f);

    //            Vector3 vectorTowardsSource = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);

    //            Vector3 directionToMe = Vector3.Normalize(transform.position - cover.transform.position);
    //            float dotToMe = Vector3.Dot(cover.transform.forward, directionToMe);

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

    //            if (currentCoverPosition != null ) currentCoverPosition.occuped = false;
    //            currentCoverPosition = coverPosition;
    //            currentCoverPosition.occuped = true;
    //            currentCoverPosition.occupedBy = character;
    //            achievedPosition = false;
    //            return cover.transform.position;
    //        }
    //    }

    //    return Vector3.zero;
    //}

    private void OnDrawGizmos() {
        if(hidingSphere == null) {
            character = GetComponentInParent<Character>();
            hidingSphere = character.GetComponentInChildren<HidingCollisionSphere>();
        }
        Gizmos.DrawWireSphere(transform.position, 36f);
        foreach (GameObject cover in hidingSphere.GetCovers().Values) {
            Vector3 direction = Vector3.Normalize(player.transform.position - cover.transform.position);
            float dotToTarget = Vector3.Dot(cover.transform.forward, direction);
            float distanceToPlayer = Vector3.Distance(player.transform.position, cover.transform.position);

            if (distanceToPlayer > 8f && dotToTarget > 0.3f) {
                Vector3 vectorTowardsPlayer = Vector3.RotateTowards(transform.forward, player.transform.position - transform.position, 999f, 999f);

                Vector3 directionToMe = Vector3.Normalize(transform.position - cover.transform.position);
                float dotToMe = Vector3.Dot(cover.transform.forward, directionToMe);

                if (Vector3.Angle(vectorTowardsPlayer, cover.transform.position - transform.position) < 60) {
                    if (dotToTarget > 0.6f && dotToMe < 0.25f) {
                        Debug.DrawLine(transform.position, cover.transform.position, Color.cyan);
                    } else {
                        Debug.DrawLine(transform.position, cover.transform.position, Color.black);
                    }
                } else {
                    Debug.DrawLine(transform.position, cover.transform.position, Color.cyan);
                }

            } else {
                Debug.DrawLine(transform.position, cover.transform.position, Color.black);
            }
        }
    }
}
