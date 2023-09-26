using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

using static Cinemachine.CinemachineOrbitalTransposer;
using static UnityEngine.UI.GridLayoutGroup;
using System.Reflection;

public class EnemyController : MonoBehaviour, ICharacterController {
    public event EventHandler OnRunStart;
    public event EventHandler OnRunCancel;

    public event EventHandler OnJumpStart;
    public event EventHandler OnJumpPerformed;
    public event EventHandler OnJumpCancel;

    public event EventHandler OnAimStart;
    public event EventHandler OnAimPerformed;
    public event EventHandler OnAimCancel;

    public event EventHandler OnCrouchStart;
    public event EventHandler OnCrouchPerformed;
    public event EventHandler OnCrouchCancel;

    public event EventHandler OnShootStart;
    public event EventHandler OnShootPerformed;
    public event EventHandler OnShootCancel;

    public event EventHandler OnFinalPositionEvent;


    public Enemy enemy { get; private set; }

    [SerializeField] private GameObject[] patrolPositions;
    [SerializeField] private Transform groundSpot;
    public Transform eyes;


    private LineRenderer pathLine;

    // Movement
    private NavMeshAgent navAgent;
    private NavMeshPath destination;

    public int currentPatrolPosition = -1;
    public bool onFinalPosition { get; private set; } = false;
    private int currentCorner = 1;

    private Vector3 moveDir;
    private IEnumerator pathRecalc;

    // AI
    public AiState state = AiState.Patrolling;
    private bool aiming = false;

    public HidingLogic hidingLogic { get; private set; }
    [SerializeField] private AttackingLogic attackingLogic;

    void Awake() {
        navAgent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        destination = new NavMeshPath();
        hidingLogic = transform.GetComponent<HidingLogic>();
        attackingLogic = transform.AddComponent<AttackingLogic>();
        enemy = transform.GetComponentInParent<Enemy>();
    }

    private void Start() {
        SetPatrollingPath();
        enemy.GetAimPosition().OnLineOfSight += SetAiState;
    }

    private void Update() {

    }
    
    public void SetAiState(object sender = null, System.EventArgs e = null) {
        //enemy.getAimPosition().OnLineOfSightLost += StopAttacking;
        switch (state) {
            case AiState.Patrolling:
            case AiState.None:
                attackingLogic.EnemySpotted();
                break;
            case AiState.Chasing:
                attackingLogic.EnemySpotted();
                break;
            case AiState.Hiding:
                attackingLogic.SetBehindCoverPosition();
                state = AiState.BehindCover;
                break;
        }
    }

    public void SetNewDestinaction(Vector3 spot) {
        currentCorner = 1;
        onFinalPosition = false;
        OnFinalPositionEvent = null;
        navAgent.CalculatePath(spot, destination);
    }

    public void FinalPosition() {
        if (OnFinalPositionEvent != null) OnFinalPositionEvent(this, EventArgs.Empty);
    }
    public void RunStart() {
        if (OnRunStart != null) OnRunStart(this, EventArgs.Empty);
    }
    public void RunCancel() {
        if (OnRunCancel != null) OnRunCancel(this, EventArgs.Empty);
    }

    public void JumpStart() {
        if (OnJumpStart != null) OnJumpStart(this, EventArgs.Empty);
    }
    public void JumpPerformed() {
        if (OnJumpPerformed != null) OnJumpPerformed(this, EventArgs.Empty);
    }
    public void JumpCancel() {
        if (OnJumpCancel != null) OnJumpCancel(this, EventArgs.Empty);
    }
    public void AimStart() {
        aiming = true;
        if (OnAimStart != null) OnAimStart(this, EventArgs.Empty);
    }
    public void AimPerformed() {
        aiming = true;
        if (OnAimPerformed != null) OnAimPerformed(this, EventArgs.Empty);
    }
    public void AimCancel() {
        aiming = false;
        if (OnAimCancel != null) OnAimCancel(this, EventArgs.Empty);
    }

    public void CrouchStart() {
        if (OnCrouchStart != null) OnCrouchStart(this, EventArgs.Empty);
    }
    public void CrouchPerformed() {
        if (OnCrouchPerformed != null) OnCrouchPerformed(this, EventArgs.Empty);
    }
    public void CrouchCancel() {
        if (OnCrouchCancel != null) OnCrouchCancel(this, EventArgs.Empty);
    }
    public void ShootStart() {
        if (OnShootStart != null) OnShootStart(this, EventArgs.Empty);
    }
    public void ShootPerformed() {
        if (OnShootPerformed != null) OnShootPerformed(this, EventArgs.Empty);
    }
    public void ShootCancel() {
        if (OnShootCancel != null) OnShootCancel(this, EventArgs.Empty);
    }

    public void SetPatrollingPath(object sender = null, System.EventArgs e = null) {
        if (patrolPositions.Length == 0) {
            onFinalPosition = true;
            state = AiState.None;
            return;
        };
        if ((currentPatrolPosition == -1 || onFinalPosition) && state == AiState.Patrolling) {
            patrolPointSelection();
            if(pathRecalc != null) StopCoroutine(pathRecalc);
            pathRecalc = RecalculatePath();
            StartCoroutine(pathRecalc);
        }
    }

    private void patrolPointSelection() {
        System.Random random = new System.Random();
        currentPatrolPosition = random.Next(0, 3);
        SetNewDestinaction(patrolPositions[currentPatrolPosition].transform.position);
        OnFinalPositionEvent += SetPatrollingPath;
    }

    private IEnumerator RecalculatePath() {
        while(!onFinalPosition) {
            currentCorner = 1;
            navAgent.CalculatePath(destination.corners[destination.corners.Length - 1], destination);
            //DrawLine();
            yield return new WaitForSeconds(.5f);
        }
    }

    private void DrawLine() {
        pathLine.positionCount = destination.corners.Length;
        pathLine.SetPosition(0, transform.parent.position);


        for(int i = 1; i< destination.corners.Length; i++) {
            var corner = destination.corners[i];
            pathLine.SetPosition(i, new Vector3(corner.x, corner.y, corner.z));

        }
    }

    public Vector2 GetMovementVectorNormalized() {

        if (state == AiState.Patrolling) SetPatrollingPath();

        if(destination == null || destination.corners.Length == 0) return Vector2.zero;

        if (Vector3.Distance(groundSpot.position, destination.corners[destination.corners.Length - 1]) < 0.2f) {
            onFinalPosition = true;
            FinalPosition();
            OnFinalPositionEvent = null;
            return new Vector2(0, 0);
        }
        moveDir = destination.corners[currentCorner] - groundSpot.position;

        if (Vector3.Distance(groundSpot.position, destination.corners[currentCorner]) < .2f && !onFinalPosition) {
            if(destination.corners.Length > 2) {
                moveDir = destination.corners[currentCorner] - groundSpot.position;
                currentCorner++;
            }
        }

        return new Vector2(moveDir.x, moveDir.z).normalized;
    }
}