using Poligon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CoverParams {

    public CoverParams(CoverPoint? coverPoint,float coverPointWidth, float maxDistanceToGround, float maxAimSightDetectionDistance, float aimHeight, float standingLeanDistance,
        float standingLeanHeight, float crouchLeanDistance, float crouchLeanHeight, bool checkEdgeCovers, float edgeCoverCheckDegree) {
        this.coverPoint = coverPoint;
        this.coverPointWidth = coverPointWidth;
        this.maxDistanceToGround = maxDistanceToGround;
        this.maxAimSightDetectionDistance = maxAimSightDetectionDistance;
        this.aimHeight  = aimHeight;
        this.standingLeanDistance   = standingLeanDistance;
        this.standingLeanHeight = standingLeanHeight;    
        this.crouchLeanDistance = crouchLeanDistance;
        this.crouchLeanHeight   = crouchLeanHeight;
        this.checkEdgeCovers    = checkEdgeCovers;
        this.edgeCoverCheckDegree   = edgeCoverCheckDegree;
    }

    public CoverPoint? coverPoint;

    public float coverPointWidth;
    public float maxDistanceToGround;


    public float maxAimSightDetectionDistance;
    public float aimHeight;
    public float standingLeanDistance;
    public float standingLeanHeight;

    public float crouchLeanDistance;
    public float crouchLeanHeight;

    public bool checkEdgeCovers;
    public float edgeCoverCheckDegree;
}
