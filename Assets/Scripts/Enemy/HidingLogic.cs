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

    [SerializeField] float minDistanceFromPlayer = 8f;
    [SerializeField] float maxCoverDistance = 16f;

    [SerializeField] HidingCollisionSphere hidingSphere;

    private Mesh mesh;

    private LineRenderer pathLine;


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

    bool isCloseCover(Vector3 position, out NavMeshPath path) {
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

    public Vector3 GetHidingPosition() {
        if (Vector3.Distance(transform.position, player.transform.position) < 20f) {
            ContactPoint[] values = hidingSphere.GetCovers().Values.ToArray();
            Array.Sort(values, (a, b) => Vector3.Distance(a.point, transform.position).CompareTo(Vector3.Distance(b.point, transform.position)));
            foreach (ContactPoint cover in values) {
                Vector3 direction = Vector3.Normalize(player.transform.position - cover.point);
                float dot = Vector3.Dot(cover.otherCollider.gameObject.transform.forward, direction);
                if (Vector3.Distance(player.transform.position, cover.point) > minDistanceFromPlayer && dot > 0.3f && isCloseCover(cover.point, out NavMeshPath path)) {
                    pathLine.positionCount = path.corners.Length;
                    pathLine.SetPosition(0, transform.position);
                    for (int i = 1; i < path.corners.Length; i++) {
                        pathLine.SetPosition(i, path.corners[i]);
                    }
                    return cover.otherCollider.gameObject.transform.position;
                }
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
        foreach (ContactPoint cover in hidingSphere.GetCovers().Values) {
            Vector3 direction = Vector3.Normalize(player.transform.position - cover.point);
            float dot = Vector3.Dot(cover.otherCollider.gameObject.transform.forward, direction);
            if (Vector3.Distance(player.transform.position, cover.point) > minDistanceFromPlayer && dot > 0.3f) {
                Debug.DrawLine(transform.position, cover.point, Color.cyan);
            } else {
                Debug.DrawLine(transform.position, cover.point, Color.black);
            }
        }
    }
}
