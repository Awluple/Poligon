using System;
using System.Collections.Generic;
using UnityEngine;
using Poligon;
using Poligon.Extensions;
using UnityEditor;
using System.Linq;

public class CoverObject : Cover {

    [SerializeField] bool ignoreInvalidPositions = false;
    [SerializeField] bool ignoreNoShotSightPositions = false;

    private float TEMPcoverPointsMinMargin;

    Collider m_Collider;
    MeshRenderer m_Renderer;
    Vector3 m_Center;
    Vector3 m_Size, m_Min, m_Max;
    bool pointsCreated = false;
    Stack<GameObject> coverPositions = new Stack<GameObject>();

    [SerializeField] GameObject coverPositionPrefab;


    public List<CoverPoint> coverPoints { get; private set; } = new List<CoverPoint>();

    private Vector3 position;
    private Vector3 scale;
    private Quaternion parentRotation;

    void Start() {
        Setup();
        if (!pointsCreated) {
            CreatePointsSketch();
        };
    }

    public void Setup() {
        TEMPcoverPointsMinMargin = coverPointsMinMargin;
        position = transform.position;

        Quaternion _originalRotation = transform.rotation;
        transform.rotation = new Quaternion(); // reset rotation to get sthe original size of the game object

        m_Renderer = GetComponent<MeshRenderer>();
        m_Collider = GetComponent<Collider>();

        m_Center = m_Collider.bounds.center;
        m_Size = m_Renderer.bounds.size;

        m_Min = m_Collider.bounds.min;
        m_Max = m_Collider.bounds.max;

        //Output this data into the console
        //OutputData();

        transform.rotation = _originalRotation;
        foreach (Transform child in transform.parent.transform) {
            if (child.TryGetComponent<CoverPosition>(out CoverPosition pos)) {
                coverPositions.Push(pos.gameObject);
            };
        }
        if(coverPositions.Count > 0) {
            pointsCreated = true;
        }
    }
    public void UpdateParent() {
        if(!transform.parent.TryGetComponent<CoverParent>(out CoverParent parent)) {
            return;
        }
        parent.transform.position = transform.position;
        transform.localPosition = Vector3.zero;
    }
    /// <summary>
    /// Run all updates for values used in calculating cover points positions (when the object moved etc)
    /// </summary>
    public void UpdateValues() {
        if (transform.position != position || (transform.parent.TryGetComponent<CoverParent>(out CoverParent parent) && transform.parent.position != position)) {
            position = transform.position;
            m_Center = m_Collider.bounds.center;
            m_Min = m_Collider.bounds.min;
            m_Max = m_Collider.bounds.max;
        }

        if(transform.localScale != scale || transform.parent.rotation != parentRotation) {
            parentRotation = transform.parent.rotation;
            Quaternion _originalRotation = transform.rotation;
            transform.rotation = Quaternion.identity;
            transform.parent.rotation = Quaternion.identity;

            m_Size = m_Renderer.bounds.size;
            transform.rotation = _originalRotation;     

            coverPoints = new List<CoverPoint>();
            scale = transform.localScale;
            if(!pointsCreated) CreatePointsSketch();
        }

        //TEMP
        if (TEMPcoverPointsMinMargin != coverPointsMinMargin) {
            TEMPcoverPointsMinMargin = coverPointsMinMargin;
            coverPoints = new List<CoverPoint>();
            if (!pointsCreated) CreatePointsSketch();
        }
    }

    private void Update() {
        UpdateValues();
    }

    public void OutputData() {
        //Output to the console the center and size of the Collider volume
        Debug.Log("Collider Center : " + m_Center);
        Debug.Log("Collider Size : " + m_Size);
        Debug.Log("Collider bound Minimum : " + m_Min);
        Debug.Log("Collider bound Maximum : " + m_Max);
    }
    /// <summary>
    /// Adds cover points along a line - recursion on both directions
    /// </summary>
    void AddPoints(float start, float end, List<CoverPoint> coverPointsLine,bool xAxis) {
        float spaceBetween = end - start;

        if(spaceBetween >= coverPointWidth + 2 * coverPointsMinMargin) { // if there is enough space to plase a point and it's margins on both sides\
            float pointPosition = start + (spaceBetween / 2);
            float positionAfterMargin = (0.5f * (coverPointWidth + coverPointsMinMargin));

            coverPointsLine.Add(new CoverPoint(new Vector3(xAxis ? pointPosition : 0, coverPointWidth / 2, xAxis ? 0 : pointPosition), 0, false));
            AddPoints(start, pointPosition - positionAfterMargin, coverPointsLine, xAxis); // left side
            AddPoints(pointPosition + positionAfterMargin, end, coverPointsLine, xAxis); // right side
        }
        return;
    }
    /// <summary>
    /// Gets possible cover positions along a line
    /// </summary>
    /// <param name="size">Line size</param>
    /// <param name="xAxis">Axis of a line, true if along X axis</param>
    /// <returns>List of cover points for the given line</returns>
    List<CoverPoint> GetPositions(float size, bool xAxis) {
        
        List<CoverPoint> coverPointsLine = new List<CoverPoint>();

        if(size >= coverPointWidth * 2 + 2 * coverPointsMinMargin) {
            float start = coverPointWidth + coverPointsMinMargin;
            float end = size - (coverPointWidth) - coverPointsMinMargin;

            // Add inner points
            AddPoints(start, end, coverPointsLine, xAxis);

            // Add first and last points
            float firstPosition = coverPointWidth / 2;
            float secondPosition = size - (coverPointWidth / 2);
            coverPointsLine.Add(new CoverPoint(new Vector3(xAxis ? firstPosition : 0, coverPointWidth / 2, xAxis ? 0 : firstPosition), 0, true));
            coverPointsLine.Add(new CoverPoint(new Vector3(xAxis ? secondPosition : 0, coverPointWidth / 2, xAxis ? 0 : secondPosition), 0 , true));

        } else if(size >= coverPointWidth) { // if there is only space for one point add it at the middle
            coverPointsLine.Add(new CoverPoint(new Vector3(xAxis ? m_Size.x / 2 : 0, coverPointWidth / 2, !xAxis ? m_Size.z / 2 : 0), 0, true));

        }
        return coverPointsLine;
    }
    /// <summary>
    /// Creates cover points for the object
    /// </summary>
    public void CreatePointsSketch() {
        coverPoints = new List<CoverPoint>();
        float width = m_Size.x;
        float height = m_Size.z;
        if(height >= coverPointWidth) {
            List<CoverPoint> coverPointsLine = GetPositions(height, false);
            coverPoints.AddRange(coverPointsLine);
        }

        if (width >= coverPointWidth) {
            List<CoverPoint> coverPointsLine = GetPositions(width, true);
            coverPoints.AddRange(coverPointsLine);
        }
    }

    /// <summary>
    /// Transforms the sketches of the cover points into Game Objects
    /// </summary>
    public void CreatePointsObjects() {
        if (pointsCreated) return;
        UpdateParent();
        m_Renderer.enabled = false;
        CoverParams coverParams = new CoverParams(null, coverPointWidth, maxDistanceToGround, maxAimSightDetectionDistance, aimHeight, standingLeanDistance,
         standingLeanHeight, crouchLeanDistance, crouchLeanHeight, checkEdgeCovers, edgeCoverCheckDegree);
        for (int mirroredSide = -1; mirroredSide <= 1; mirroredSide = mirroredSide + 2) {
            foreach (CoverPoint coverPoint in GetCurrentPosition(-mirroredSide)) {
                coverParams.coverPoint = coverPoint;

                // Ignore invalid positions if requested
                Color color = Color.white;
                Vector3 dir;
                if (coverPoint.axis == CoverPointAxis.Y) {
                    dir = transform.forward * mirroredSide;
                } else {
                    dir = transform.right * mirroredSide;
                }
                List<Action> debugRays = new List<Action>();
                List<CoverPose> coverPointShotPositions = GetShootingPositions(coverPoint, mirroredSide, debugRays);

                if (coverPointShotPositions.Count == 0 && debugRays.Count > 0) {
                    color = noShotSightColor;
                }
                if(debugRays.Count == 0) {
                    color = positioInvalidColor;
                }
                if (Physics.CheckSphere(coverPoint.position, coverPointWidth / 2 - 0.1f)) {
                    color = positioInvalidColor;

                }
                if (!Physics.Raycast(coverPoint.position, Vector3.down, maxDistanceToGround)) {
                    color = positioInvalidColor;

                }
                if (ignoreNoShotSightPositions && color == noShotSightColor) { color = positionFreeColor; continue; };
                if (ignoreInvalidPositions && color == positioInvalidColor) { color = positionFreeColor; continue; }

                // Create positions
                var point = coverPositionPrefab.Instantiate(coverPositionPrefab, coverPoint.position, Quaternion.FromToRotation(Vector3.forward, dir), coverParams);
                if (point.transform.rotation.eulerAngles.z < 0) {
                    Vector3 angles = point.transform.rotation.eulerAngles;
                    angles.x = 180;
                    point.transform.rotation = Quaternion.Euler(angles);
                }
                point.transform.SetParent(transform.parent.transform, true);
                point.layer = 20;
                coverPositions.Push(point);
            }
        }
        pointsCreated = true;
        coverPoints = new List<CoverPoint>();
    }
    /// <summary>
    /// Removes all cover points Game Objects add redraws points sketches
    /// </summary>
    public void ResetCoverPoints() {
        m_Renderer.enabled = true;
        while (coverPositions.Count > 0) {
            GameObject position = coverPositions.Pop();
            DestroyImmediate(position.gameObject);
        }
        pointsCreated = false;
        CreatePointsSketch();
    }

    /// <summary>
    /// Calculates the current position of the cover points fot the given siede
    /// </summary>
    /// <param name="coverSide">1 or -1 depending on which side of an object the calculation needs to be made</param>
    /// <returns>Current cover points positions</returns>
    IEnumerable<CoverPoint> GetCurrentPosition(int coverSide) {
        foreach (CoverPoint vec in coverPoints) {
            CoverPoint coverPoint = vec;
            Vector3 pos = vec.position;
            pos.y = pos.y + (transform.position.y - coverPointWidth / 2);
            pos.y += yAxisMargin;
            if (vec.position.x != 0) {
                float start = m_Center.x - m_Size.x / 2;
                pos.x = pos.x + start;
                pos.z = transform.position.z + coverSide*(m_Size.z / 2) + coverSide * (coverPointWidth / 2) + coverSide * distanceMargin;
                pos = RotatePointAroundPivot(pos, transform.rotation, m_Center);
                coverPoint.axis = CoverPointAxis.Y;
            } else {
                float start = m_Center.z - m_Size.z / 2;
                pos.z = pos.z + start;
                pos.x = transform.position.x + coverSide * (m_Size.x / 2) + coverSide * (coverPointWidth / 2) + coverSide * distanceMargin;
                pos = RotatePointAroundPivot(pos, transform.rotation, m_Center);
                coverPoint.axis = CoverPointAxis.X;
            }
            coverPoint.position = pos;
            yield return coverPoint;
        }
    }


    #if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
        if (pointsCreated) return;
        Gizmos.color = positionFreeColor;

        for (int mirroredSide = -1; mirroredSide <= 1; mirroredSide = mirroredSide + 2) {
            foreach (CoverPoint coverPoint in GetCurrentPosition(-mirroredSide)) {
                List<Action> debugRays = new List<Action>();
                List<CoverPose> coverPointShotPositions = GetShootingPositions(coverPoint, mirroredSide, debugRays);

                if (coverPointShotPositions.Count == 0 && debugRays.Count > 0) {
                    Gizmos.color = noShotSightColor;
                }

                if (Physics.CheckSphere(coverPoint.position, coverPointWidth / 2 - 0.1f, 0)) {
                    Gizmos.color = positioInvalidColor;
                }
                if (!Physics.Raycast(coverPoint.position, Vector3.down, maxDistanceToGround)) {
                    debugRays.Add(() => { Debug.DrawRay(coverPoint.position, Vector3.down * 2f, Color.red); });
                    Gizmos.color = positioInvalidColor;
                }
                if (ignoreNoShotSightPositions && Gizmos.color == noShotSightColor) { Gizmos.color = positionFreeColor; continue; };
                if (ignoreInvalidPositions && Gizmos.color == positioInvalidColor) { Gizmos.color = positionFreeColor; continue; };

                foreach (Action action in debugRays) {
                    action();
                }
                Gizmos.DrawWireSphere(coverPoint.position, coverPointWidth / 2);
                Gizmos.color = positionFreeColor;
            }
        }
    }
    #endif
}
