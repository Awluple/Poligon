using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            Color color = edge.distance > .5f ? Color.red : Color.blue;
            //Debug.DrawLine(e1, e2, color, 10f);
            //Debug.DrawRay(edge.middle, edge.forward, Color.green, 10f);
        }
        var positions = DynamicCover.GetValidHidingPositions(hidingSource.transform.position, hidingSource, edges, transform.position);
        if (positions.Count > 0) {
            var position = DynamicCover.GetBestPosition(hidingSource.transform.position, positions, agent);
        } else {
            Debug.Log("No position!");
        }
        
    }
}
