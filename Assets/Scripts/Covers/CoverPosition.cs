using Poligon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
public class CoverPosition : Cover
{
    [SerializeField] bool isEdgeCover;

    [SerializeField] List<CoverPose> avaliablePoses =  new List<CoverPose>();
    private bool posesChecked = false;
    public bool occuped = false;

    public void Setup(CoverParams coverParams) {
        isEdgeCover = coverParams.coverPoint.GetValueOrDefault().isEdgeCover;
        coverPointWidth = coverParams.coverPointWidth;
        maxDistanceToGround = coverParams.maxDistanceToGround;
        maxAimSightDetectionDistance = coverParams.maxAimSightDetectionDistance;
        aimHeight = coverParams.aimHeight;
        standingLeanDistance = coverParams.standingLeanDistance;
        standingLeanHeight = coverParams.standingLeanHeight;
        crouchLeanDistance = coverParams.crouchLeanDistance;
        crouchLeanHeight = coverParams.crouchLeanHeight;
        checkEdgeCovers = coverParams.checkEdgeCovers;
        edgeCoverCheckDegree = coverParams.edgeCoverCheckDegree;
    }

    public List<CoverPose> GetCoverPoses() {
        if (posesChecked == false) {
            avaliablePoses = GetShootingPositions(new CoverPoint(transform.position, CoverPointAxis.Y, isEdgeCover), 1);
            avaliablePoses = avaliablePoses.Distinct().ToList();
            posesChecked = true;
        }
        return avaliablePoses;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;

        List<Action> debugRays = new List<Action>();
        List<CoverPose> poses = GetShootingPositions(new CoverPoint(transform.position, CoverPointAxis.Y, isEdgeCover), 1, debugRays);

        foreach(Action ray in debugRays) {
            ray();
        }

        Gizmos.DrawWireSphere(transform.position, coverPointWidth / 2);

    }
#endif
}
