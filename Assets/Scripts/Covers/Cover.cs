using Poligon;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cover : MonoBehaviour
{
    [SerializeField] protected float coverPointWidth = 2f;
    [SerializeField] protected float maxDistanceToGround = 3f;


    [Range(-4, 4)]
    [SerializeField] protected float yAxisMargin = 0f;
    [Range(-4, 4)]
    [SerializeField] protected float distanceMargin = 0f;
    [Range(0, 4)]
    [SerializeField] protected float coverPointsMinMargin = 0.5f;

    [Range(-2, 2)]
    [SerializeField] protected float coverDetectionDistance = 0f;
    [Range(0, 80)]
    [SerializeField] protected float maxAimSightDetectionDistance = 5f;
    [Range(-4, 4)]
    [SerializeField] protected float aimHeight = 1f;
    [Range(0, 4)]
    [SerializeField] protected float standingLeanDistance = 1f;
    [Range(0, 4)]
    [SerializeField] protected float standingLeanHeight = 1f;

    [Range(0, 4)]
    [SerializeField] protected float crouchLeanDistance = 1f;
    [Range(0, 4)]
    [SerializeField] protected float crouchLeanHeight = 0f;

    [SerializeField] protected bool checkEdgeCovers = true;
    [Range(0, 90)]
    [SerializeField] protected float edgeCoverCheckDegree = 45f;

    [SerializeField] protected Color positionFreeColor = new Color(76 / 255f, 135 / 255f, 58 / 255f);
    [SerializeField] protected Color positioInvalidColor = Color.red;
    [SerializeField] protected Color noShotSightColor = Color.yellow;

    /// <summary>
    /// Gets starting positions for rays while leaning
    /// </summary>
    /// <param name="coverPoint">Position of a cover point</param>
    /// <param name="height">Lean height</param>
    /// <param name="distance">Lean distance</param>
    /// <returns>Starting position of a ray</returns>
    protected Vector3 GetRayPos(CoverPoint coverPoint, float height, float distance) {
        Vector3 rayPos = coverPoint.position;
        rayPos.y = coverPoint.position.y + height;
        if (coverPoint.axis != CoverPointAxis.Y) {
            rayPos.z = coverPoint.position.z + distance;
        } else {
            rayPos.x = coverPoint.position.x + distance;
        }
        rayPos = RotatePointAroundPivot(rayPos, transform.rotation, coverPoint.position);

        return rayPos;
    }
    /// <summary>
    /// Adds DrawRay delegates to debugRays and checks if it is possible to shoot from this position
    /// </summary>
    /// <param name="coverPoint">Position of a cover point</param>
    /// <param name="endPointAlign">Align of the ray: 1 or -1 depending on which side of a cover point the end point is</param>
    /// <param name="dir">Direction of the ray to be created</param>
    /// <param name="height">Lean height</param>
    /// <param name="distance">Lean distance</param>
    /// <param name="debugRays">List of DrawRay delegates to draw rays</param>
    /// <returns>List of avaliable positions</returns>
    protected List<CoverPose> GetCoverViewRays(CoverPoint coverPoint, int endPointAlign, Vector3 dir, float height, float distance, List<Action> debugRays = null) {
        List<CoverPose> coverPointShotPositions = new List<CoverPose>();
        for (int i = -1; i <= 1; i = i + 2) {
            Vector3 rayPos = GetRayPos(coverPoint, height, i * distance);
            if (Physics.Raycast(rayPos, endPointAlign * dir, maxAimSightDetectionDistance)) {// try right lean pose
                debugRays?.Add(() => { Debug.DrawRay(rayPos, 2 * endPointAlign * dir, Color.red); });
            } else {
                debugRays?.Add(() => { Debug.DrawRay(rayPos, 2 * endPointAlign * dir, Color.blue); });
                if (i * endPointAlign * distance < 0) { // if on the right
                    coverPointShotPositions.Add(height == standingLeanHeight ? CoverPose.RightStanding : CoverPose.RightCrouch);
                } else { // if on the left
                    coverPointShotPositions.Add(height == standingLeanHeight ? CoverPose.LeftStanding : CoverPose.LeftCrouch);
                }
            }
        }
        return coverPointShotPositions;
    }
    /// <summary>
    /// (99% it can be done better) Adds DrawRay delegates to debugRays and checks if it is possible to shoot from this position
    /// </summary>
    /// <param name="coverPoint">Position of a cover point</param>
    /// <param name="endPointAlign">Align of the ray: 1 or -1 depending on which side of a cover point the end point is</param>
    /// <param name="side">Side of the cover point (1 or 2)</param>
    /// <param name="dir">Direction of the ray to be created</param>
    /// <param name="height">Lean height</param>
    /// <param name="distance">Lean distance</param>
    /// <param name="debugRays">List of DrawRay delegates to draw rays</param>
    /// <returns>List of avaliable positions</returns>
    protected List<CoverPose> GetCornerCoverViewRays(CoverPoint coverPoint, int endPointAlign, int side, Vector3 dir, float height, float distance, List<Action> debugRays = null) {
        List<CoverPose> coverPointShotPositions = new List<CoverPose>();

        int coverPointSide = (side == 1 ? 1 : -1); // left or right side of the cover point
        Vector3 transformDir = Vector3.up;
        float finalRotation = edgeCoverCheckDegree;

        if (side == 2) { // if it the the second side of a cover point
            finalRotation = -finalRotation;
        }
        if (coverPoint.axis == CoverPointAxis.Y) {
            finalRotation = -finalRotation;
        }
        if (endPointAlign == 1) { // if it the the mirrored side the cover
            finalRotation = -finalRotation;
        }

        Quaternion rotation = Quaternion.Euler(transform.TransformDirection(transformDir) * finalRotation);
        dir = rotation * dir;
        Vector3 rayPos = GetRayPos(coverPoint, height, coverPointSide * distance);

        if (Physics.Raycast(rayPos, endPointAlign * dir, maxAimSightDetectionDistance)) {
            debugRays?.Add(() => { Debug.DrawRay(rayPos, 2 * endPointAlign * dir, Color.red); });
        } else {
            debugRays?.Add(() => { Debug.DrawRay(rayPos, 2 * endPointAlign * dir, Color.blue); });
            if (endPointAlign * coverPointSide * distance < 0) { // if on the right
                coverPointShotPositions.Add(height == standingLeanHeight ? CoverPose.RightStanding : CoverPose.RightCrouch);
            } else { // if on the left
                coverPointShotPositions.Add(height == standingLeanHeight ? CoverPose.LeftStanding : CoverPose.LeftCrouch);
            }
        }
        return coverPointShotPositions;
    }
    /// <summary>
    /// Get the avaliable shooting positions for the given coverPoint
    /// </summary>
    /// <param name="coverPoint">The cover point to check</param>
    /// <param name="mirroredSide">Side of the cover object</param>
    /// <param name="debugRays">Rays of shooting positions for debugging</param>
    /// <returns>List of avaliavle shooting positions</returns>
    protected List<CoverPose> GetShootingPositions(CoverPoint coverPoint, int mirroredSide, List<Action> debugRays = null) {
        List<CoverPose> coverPointShotPositions = new List<CoverPose>();

        Gizmos.color = positionFreeColor;
        Vector3 pos = coverPoint.position;
        for (int side = 1; side <= 2; side++) {
            Vector3 dir = Vector3.zero;
            if (coverPoint.axis == CoverPointAxis.Y) {
                dir = transform.forward;
            } else if (coverPoint.axis == CoverPointAxis.X) {
                dir = transform.right;
            }
            Vector3 rayPos = pos;
            rayPos.y += coverDetectionDistance;
            if (!Physics.Raycast(rayPos, mirroredSide * dir, distanceMargin + 2.5f)) {// check if there is a cover and not empty spot
                Gizmos.color = positioInvalidColor;
                return coverPointShotPositions;
            }
            rayPos.y += (aimHeight - coverDetectionDistance);
            if (!Physics.Raycast(rayPos, mirroredSide * dir, maxAimSightDetectionDistance)) {// try standard standing pose
                debugRays?.Add(() => { Debug.DrawRay(rayPos, mirroredSide * dir * 2, Color.blue); });
                coverPointShotPositions.Add(CoverPose.Standing);

            } else { // get rays for lean poses
                coverPointShotPositions.AddRange(GetCoverViewRays(coverPoint, mirroredSide, dir, standingLeanHeight, standingLeanDistance, debugRays));
                coverPointShotPositions.AddRange(GetCoverViewRays(coverPoint, mirroredSide, dir, crouchLeanHeight, crouchLeanDistance, debugRays));
                if (coverPoint.isEdgeCover && checkEdgeCovers) {
                    coverPointShotPositions.AddRange(GetCornerCoverViewRays(coverPoint, mirroredSide, side, dir, standingLeanHeight, standingLeanDistance, debugRays));
                    coverPointShotPositions.AddRange(GetCornerCoverViewRays(coverPoint, mirroredSide, side, dir, crouchLeanHeight, crouchLeanDistance, debugRays));

                }
            }
        }
        return coverPointShotPositions;
    }

    public Vector3 RotatePointAroundPivot(Vector3 _finalPosition, Quaternion _finalRotation, Vector3 _pivotPosition) {
        return _pivotPosition + (_finalRotation * (_finalPosition - _pivotPosition));
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 angles, Vector3 pivot) {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
}
