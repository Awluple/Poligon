using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Poligon;
using Poligon.Ai;
public class DebugDynamicHiding : MonoBehaviour
{
    [SerializeField] DynamicCover DynamicCover;
    private List<Edge> edges;
    private bool ready = false;
    private NavMeshAgent agent;
    [SerializeField] Character hidingSource;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            GetEdges();
        }
        if (ready) {
            ShowEdges();
        }
    }
    async Awaitable GetEdges() {
        edges = await DynamicCover.GetCovers(transform.position, agent);
        ready = true;
    }
    void ShowEdges() {
        ready = false;
        
        foreach (var edge in edges) {
            var e1 = edge.point1;
            e1.y += 1f;
            var e2 = edge.point2;
            e2.y += 1f;
            Color color = edge.length > .5f ? Color.red : Color.blue;
            Debug.DrawLine(e1, e2, color, 10f);
            //Debug.DrawRay(edge.middle, edge.forward, Color.green, 10f);
        }
        var positions = DynamicCover.GetValidHidingPositions(hidingSource.transform.position, edges, transform.position);
        if (positions.Count > 0) {
            var position = DynamicCover.GetBestPosition(hidingSource.transform.position, positions, agent);
            List<(float, SubEdge)> distances = new();
            NavMeshPath path = new NavMeshPath();
            foreach(var subEdge in position.subEdges) {
                agent.CalculatePath(subEdge.middle, path);
                distances.Add((DynamicCover.CalculatePath(path.corners), subEdge));
            }
            var coverPosition = distances.Where(d => !d.Item2.occupied).OrderBy(s => s.Item1).Select(d=> d.Item2).First();
            Debug.DrawLine(coverPosition.start, coverPosition.end, Color.green, 10f);

        } else {
            Debug.Log("No position!");
        }
        
    }
}
