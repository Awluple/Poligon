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

    private event EventHandler OnFinalPositionEvent;


    [SerializeField] private Enemy enemy;

    [SerializeField] private GameObject[] patrolPositions;
    [SerializeField] private Transform groundSpot;
    [SerializeField] private Transform eyes;


    private LineRenderer pathLine;

    // Movement
    private NavMeshAgent navAgent;
    private NavMeshPath destination;

    private int currentPatrolPosition = -1;
    private bool onFinalPosition = false;
    private int currentCorner = 1;

    private Vector3 moveDir;
    private IEnumerator pathRecalc;
    private IEnumerator attackCoroutine;
    private IEnumerator checkLastSeen;

    // AI
    [SerializeField]private AiState state = AiState.Patrolling;
    private float lastSeen;
    private Vector3 lastKnownPosition;
    private bool alerted = false;
    private bool aiming = false;

    private HidingLogic hidingLogic;

    void Awake() {
        navAgent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        destination = new NavMeshPath();
        hidingLogic = transform.GetComponent<HidingLogic>();
    }

    private void Start() {
        SetPatrollingPath();
        enemy.getAimPosition().OnLineOfSight += SetAttackingState;
    }

    private void Update() {
        Debug.Log(state);
    }
    
    public void SetAttackingState(object sender = null, System.EventArgs e = null) {
        alerted = true;
        //enemy.getAimPosition().OnLineOfSightLost += StopAttacking;
        switch (state) {
            case AiState.Patrolling:
                EnemySpotted();
                break;
            case AiState.Chasing:
                EnemySpotted();
                break;
            case AiState.Hiding:
                state = AiState.BehindCover;
                break;
        }
    }

    public void StopAttacking(object sender = null, System.EventArgs e = null) {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (attackCoroutine != null) StopCoroutine(checkLastSeen);
        alerted = false;
        AimCancel();
        state = AiState.Patrolling;
        currentPatrolPosition = -1;
        SetPatrollingPath();
    }

    private void EnemySpotted() {
        Vector3 hidingSpot = hidingLogic.GetHidingPosition();

        if(checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        checkLastSeen = CheckLastSeen();
        StartCoroutine(checkLastSeen);
        state = AiState.Hiding;


        if (hidingSpot != Vector3.zero ) {
            //AimStart();
            SetNewDestinaction(hidingSpot);
        } else {
            SetNewDestinaction(transform.position);
        }
        OnFinalPositionEvent += SetAttackingState;
        OnFinalPositionEvent += (object sender, System.EventArgs e) => { RunCancel(); };

        //AimStart();
        if(attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = ContinueAttacking();
        StartCoroutine(attackCoroutine);
    }

    IEnumerator CheckLastSeen() {
        for (; ; ) {
            if (Time.time - lastSeen > 10f && state != AiState.Chasing) {
                state = AiState.Chasing;
                SetNewDestinaction(lastKnownPosition);
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                RunCancel();
                enemy.getAimPosition().Reset();
            } else if (Time.time - lastSeen > 20f) {
                StopAttacking();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ContinueAttacking() {
        for (; ; ) {
            Ray ray = new Ray(eyes.transform.position, enemy.getAimPosition().transform.position - eyes.transform.position);

            if (Physics.Raycast(ray, out RaycastHit hit, 40f)) {
                if(hit.collider.gameObject.TryGetComponent<Character>(out Character character)) {
                    if (!aiming) AimStart();
                    if(character == enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...
                    lastSeen = Time.time;
                    lastKnownPosition = character.transform.position;
                    ShootPerformed();
                } else if(state != AiState.Chasing) {
                    AimCancel();
                }
                
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void SetNewDestinaction(Vector3 spot) {
        currentCorner = 1;
        onFinalPosition = false;
        OnFinalPositionEvent = null;
        navAgent.CalculatePath(spot, destination);
    }

    private void FinalPosition() {
        if (OnFinalPositionEvent != null) OnFinalPositionEvent(this, EventArgs.Empty);
    }
    private void RunStart() {
        if (OnRunStart != null) OnRunStart(this, EventArgs.Empty);
    }
    private void RunCancel() {
        if (OnRunCancel != null) OnRunCancel(this, EventArgs.Empty);
    }

    private void JumpStart() {
        if (OnJumpStart != null) OnJumpStart(this, EventArgs.Empty);
    }
    private void JumpPerformed() {
        if (OnJumpPerformed != null) OnJumpPerformed(this, EventArgs.Empty);
    }
    private void JumpCancel() {
        if (OnJumpCancel != null) OnJumpCancel(this, EventArgs.Empty);
    }
    private void AimStart() {
        aiming = true;
        if (OnAimStart != null) OnAimStart(this, EventArgs.Empty);
    }
    private void AimPerformed() {
        aiming = true;
        if (OnAimPerformed != null) OnAimPerformed(this, EventArgs.Empty);
    }
    private void AimCancel() {
        aiming = false;
        if (OnAimCancel != null) OnAimCancel(this, EventArgs.Empty);
    }

    private void CrouchStart() {
        if (OnCrouchStart != null) OnCrouchStart(this, EventArgs.Empty);
    }
    private void CrouchPerformed() {
        if (OnCrouchPerformed != null) OnCrouchPerformed(this, EventArgs.Empty);
    }
    private void CrouchCancel() {
        if (OnCrouchCancel != null) OnCrouchCancel(this, EventArgs.Empty);
    }
    private void ShootStart() {
        if (OnShootStart != null) OnShootStart(this, EventArgs.Empty);
    }
    private void ShootPerformed() {
        if (OnShootPerformed != null) OnShootPerformed(this, EventArgs.Empty);
    }
    private void ShootCancel() {
        if (OnShootCancel != null) OnShootCancel(this, EventArgs.Empty);
    }

    private void SetPatrollingPath(object sender = null, System.EventArgs e = null) {
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