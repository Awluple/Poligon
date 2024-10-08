using Poligon;
using Poligon.Ai.States;
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
    [SerializeField] private bool _occuped = false;
    public bool occuped {
        get => _occuped; 
        set {
            Material myMaterial = GetComponent<Renderer>().material;
            Color ocupp = new Color(0.4f, 0.9f, 0.7f, 1.0f);
            Color free = new Color(0.7f, 0.3f, 0.5f, 1.0f);
            if (value == false) {
                occupedBy = null;
                myMaterial.color = free;
            } else {
                myMaterial.color = ocupp;
            }
            _occuped = value;
        }
    }
    public Character occupedBy = null;

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

    public void Start() {
        Material myMaterial = GetComponent<Renderer>().material;
        Color free = new Color(0.7f, 0.3f, 0.5f, 1.0f);
        myMaterial.color = free;
        GetComponent<Renderer>().enabled = false;
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
