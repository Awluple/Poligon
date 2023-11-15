using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.UI.GridLayoutGroup;

public class HidingLogic : MonoBehaviour {
    public Character character;
    private Player player;

    private NavMeshAgent agent;
    [SerializeField] private LayerMask hidableLayers;

    [SerializeField] HidingCollisionSphere hidingSphere;

    private Mesh mesh;

    private LineRenderer pathLine;

    public CoverPosition currentCoverPosition { get; private set; }


    private void Awake() {
        agent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        mesh = new Mesh();
        player = FindObjectOfType<Player>();

        NavMeshTriangulation navmeshData = NavMesh.CalculateTriangulation();

        mesh.SetVertices(navmeshData.vertices.ToList());
        mesh.SetIndices(navmeshData.indices, MeshTopology.Triangles, 0);

        character = GetComponentInParent<Character>();
        hidingSphere = character.GetComponentInChildren<HidingCollisionSphere>();


        pathLine = gameObject.AddComponent<LineRenderer>();
        pathLine.startWidth = 0.2f;
        pathLine.endWidth = 0.2f;
        pathLine.positionCount = 0;

    }

    bool isCloseCover(Vector3 position, float maxCoverDistance, out NavMeshPath path) {
        path = new NavMeshPath();

        if(!agent.CalculatePath(position, path)) {
            return false;
        }
        float distance = 0;
        for(int i = 0; i < path.corners.Length; i++) {
            if (i + 1 > path.corners.Length - 1) break;
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return distance < maxCoverDistance;
    }

    public Vector3 GetHidingPosition(Vector3 hidingSourcePosition, Vector3? originPosition = null, bool closestCover = true, bool ignoreDot = false, float minDistanceFromSource = 8f, float maxCoverPathDistance = 20f, float maxSearchDistance = 21f) {
        Vector3 originPos = Vector3.zero;
        if (originPosition == null) originPos = transform.position;
        else { originPos = originPosition.GetValueOrDefault(); }

        GameObject[] values = hidingSphere.GetCovers().Values.ToArray();
        if (closestCover) {
            Array.Sort(values, (a, b) => Vector3.Distance(a.transform.position, originPos).CompareTo(Vector3.Distance(b.transform.position, originPos)));
        } else {
            Array.Sort(values, (a, b) => Vector3.Distance(b.transform.position, originPos).CompareTo(Vector3.Distance(a.transform.position, originPos)));
        }
        foreach (GameObject cover in values) {
            if (Vector3.Distance(originPos, cover.transform.position) > maxSearchDistance) { continue; };

            CoverPosition coverPosition = cover.GetComponent<CoverPosition>();
            if (coverPosition.occuped) continue;

            Vector3 direction = Vector3.Normalize(hidingSourcePosition - cover.transform.position);
            float dotToPlayer = Vector3.Dot(cover.transform.forward, direction);

            if (Vector3.Distance(hidingSourcePosition, cover.transform.position) > minDistanceFromSource && dotToPlayer > 0.3f && isCloseCover(cover.transform.position, maxCoverPathDistance, out NavMeshPath path)) {

                Vector3 towards = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);
                Debug.DrawRay(transform.position, towards * 10, Color.red, 40f);

                Vector3 vectorTowardsSource = Vector3.RotateTowards(transform.forward, hidingSourcePosition - transform.position, 999f, 999f);

                Vector3 directionToMe = Vector3.Normalize(transform.position - cover.transform.position);
                float dotToMe = Vector3.Dot(cover.transform.forward, directionToMe);

                if (!ignoreDot && Vector3.Angle(vectorTowardsSource, cover.transform.position - transform.position) < 60) {
                    if (dotToPlayer > 0.6f && dotToMe < 0.25f) {

                    } else {
                        continue;
                    }
                } else {

                }

                pathLine.positionCount = path.corners.Length;
                pathLine.SetPosition(0, transform.position);
                for (int i = 1; i < path.corners.Length; i++) {
                    pathLine.SetPosition(i, path.corners[i]);
                }
                if(currentCoverPosition != null ) currentCoverPosition.occuped = false;
                currentCoverPosition = coverPosition;
                currentCoverPosition.occuped = true;
                return cover.transform.position;
            }
        }

        return Vector3.zero;
    }

    private void OnDrawGizmos() {
        if(hidingSphere == null) {
            character = GetComponentInParent<Character>();
            hidingSphere = character.GetComponentInChildren<HidingCollisionSphere>();
        }
        Gizmos.DrawWireSphere(transform.position, 36f);
        foreach (GameObject cover in hidingSphere.GetCovers().Values) {
            Vector3 direction = Vector3.Normalize(player.transform.position - cover.transform.position);
            float dotToPlayer = Vector3.Dot(cover.transform.forward, direction);
            float distanceToPlayer = Vector3.Distance(player.transform.position, cover.transform.position);

            if (distanceToPlayer > 8f && dotToPlayer > 0.3f) {
                Vector3 vectorTowardsPlayer = Vector3.RotateTowards(transform.forward, player.transform.position - transform.position, 999f, 999f);

                Vector3 directionToMe = Vector3.Normalize(transform.position - cover.transform.position);
                float dotToMe = Vector3.Dot(cover.transform.forward, directionToMe);

                if (Vector3.Angle(vectorTowardsPlayer, cover.transform.position - transform.position) < 60) {
                    if (dotToPlayer > 0.6f && dotToMe < 0.25f) {
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
