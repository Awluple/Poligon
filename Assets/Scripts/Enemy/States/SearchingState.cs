using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;

namespace Poligon.Ai.EnemyStates {
    public class SearchingState : EnemyBaseState {
        static bool navmeshReady = false;
        static NavMeshTriangulation navMeshData;
        public static List<SearchingArea> areas = new();
        private (SearchingArea area, int index) currentArea;
        private Vector3[] corners;

        public SearchingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Searching;

        public override void EnterState() {
            enemyController.CrouchCancel();
            GetPoints();

            for(int i = 0; i < areas.Count; i++) {
                var area = areas[i];
                if (Physics.Raycast(enemyController.eyes.transform.position, (area.Position - enemyController.eyes.transform.position), out RaycastHit hit, 15f)){
                    
                    if (Vector3.Distance(hit.point, enemyController.eyes.transform.position) +10f > Vector3.Distance(enemyController.eyes.transform.position, area.Position)) {
                        area.AreaChecked = true;
                        areas[i] = area;
                    }
                }
            }

            (SearchingArea area, int index) nextArea = GetNextArea();

            corners = enemyController.SetNewDestinaction(nextArea.area.Position);
            enemyController.OnFinalPositionEvent += AreaReached;

            enemyController.OnFinalPositionEvent += NextCorner;
            enemyController.OnNextCornerEvent += NextCorner;

            currentArea = nextArea;

            currentArea.area.AreaChecked = true;
            areas[currentArea.index] = currentArea.area;


            enemyController.AimStart();

            Vector3 pos = corners[enemyController.currentCorner];
            Vector3 direction = (enemyController.transform.position - pos).normalized * -1.5f;
            pos.y += 1.4f;
            pos += direction;
            enemyController.enemy.GetAimPosition().Reposition(pos);

            enemyController.enemy.GetAimPosition().OnLineOfSight += Hide;

        }

        public override void ExitState() {
            enemyController.enemy.GetAimPosition().OnLineOfSight -= Hide;
        }

        private void Hide(object sender, EventArgs args) {
            if(enemyController.aiState == AiState.Searching) enemyController.aiState = AiState.Hiding;
        }

        public void NextCorner(object sender = null, EventArgs args = null) {
            Vector3 pos = corners[enemyController.currentCorner];
            Vector3 direction = (enemyController.transform.position - pos).normalized * -2.5f;
            pos.y += 1.6f;
            pos += direction;


            enemyController.enemy.GetAimPosition().MoveAim(pos, 10f);

            for (int i = 0; i < areas.Count; i++) {
                var area = areas[i];
                if (Physics.Raycast(enemyController.eyes.transform.position, (area.Position - enemyController.eyes.transform.position), out RaycastHit hit, 15f)) {

                    if (Vector3.Distance(hit.point, enemyController.eyes.transform.position) + 5f > Vector3.Distance(enemyController.eyes.transform.position, area.Position)) {
                        area.AreaChecked = true;
                        areas[i] = area;
                    }
                }
            }

        }

        private (SearchingArea area, int index) GetNextArea() {
            (SearchingArea area, int index) area = areas.Select((area, index) => (area, index)).Where(a => a.area.AreaChecked == false).OrderBy(a => Vector3.Distance(enemyController.enemy.squad.GetChasingLocation().position, a.area.Position)).First();
            NavMeshPath navMeshPath = new NavMeshPath();
            if (enemyController.navAgent.CalculatePath(area.area.Position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete) {
                return area;
            } else {
                area.area.AreaChecked = true;
                areas[area.index] = area.area;
                return GetNextArea();
            }
        }

        private void AreaReached(object sender = null, EventArgs args = null) {
            
            (SearchingArea area, int index) nextArea = GetNextArea();

            currentArea = nextArea;
            currentArea.area.AreaChecked = true;
            areas[currentArea.index] = currentArea.area;

            corners = enemyController.SetNewDestinaction(nextArea.area.Position);
            enemyController.OnFinalPositionEvent += AreaReached;
            enemyController.OnFinalPositionEvent += NextCorner;
            enemyController.OnNextCornerEvent += NextCorner;

        }


        private List<SearchingArea> GetPoints() {
            if (!navmeshReady) {
                navMeshData = NavMesh.CalculateTriangulation();
                navmeshReady = true;
            }

            if (areas.Count > 0) return areas;

            for (int i = 0; i < navMeshData.indices.Length; i += 3) {
                Vector3 p1 = navMeshData.vertices[navMeshData.indices[i]];
                Vector3 p2 = navMeshData.vertices[navMeshData.indices[i + 1]];
                Vector3 p3 = navMeshData.vertices[navMeshData.indices[i + 2]];

                Vector3 triangleCenter = (p1 + p2 + p3) / 3f;

                float a = Vector3.Distance(p1, p2);
                float b = Vector3.Distance(p1, p3);
                float c = Vector3.Distance(p2, p3);

                float circ = (a + b + c) / 2;

                float triangleAreaSize = Mathf.Sqrt(circ * (circ - a) * (circ - b) * (circ - c));

                if (triangleAreaSize < 10 || Vector3.Distance(enemyController.transform.position, triangleCenter) > 175f) {
                    continue;
                }

                if (Physics.Raycast(triangleCenter, Vector3.down, out RaycastHit hit, Mathf.Infinity, NavMesh.AllAreas)) {
                    Vector3 position = hit.point;
                    position.y = hit.point.y + 0f;
                    areas.Add(new SearchingArea(position, false, triangleAreaSize > 100));
                }

            }
            return areas;
        }
    }
}