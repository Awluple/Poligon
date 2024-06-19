using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Poligon.Ai;
using Poligon.Ai.EnemyStates;


using static Cinemachine.CinemachineOrbitalTransposer;
using static UnityEngine.UI.GridLayoutGroup;
using System.Reflection;
using UnityEngine.InputSystem.XR;
using Poligon.Ai.Commands;

public class EnemyController : MonoBehaviour, IAICharacterController, IStateManager {
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

    public event EventHandler OnReloadStart;
    public event EventHandler OnReloadCancel;

    public event EventHandler OnFinalPositionEvent;

    public event EventHandler OnNextCornerEvent;


    public Enemy enemy { get; private set; }

    [SerializeField] private bool aiEnabled = true;


    public GameObject[] patrolPositions;
    [SerializeField] private Transform groundSpot;
    public Transform eyes;


    private LineRenderer pathLine;

    // Movement
    public NavMeshAgent navAgent { get; private set; }
    public NavMeshPath destination;

    public int currentPatrolPosition = -1;
    public bool onFinalPosition { get; set; } = false;
    public int currentCorner { get; set; } = 1;

    private Vector3 moveDir;
    private IEnumerator pathRecalc;

    // AI
    public AiState aiState { get => stateMashine.GetState(); set {
            stateMashine.MoveNext(value);
        } }
    private AiStateMashine<AiState> stateMashine;
    private Action stateMashineCallback;

    public HidingLogic hidingLogic { get; private set; }
    public AttackingLogic attackingLogic { get; private set; }

    void Awake() {
        navAgent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        destination = new NavMeshPath();
        hidingLogic = transform.GetComponent<HidingLogic>();
        attackingLogic = transform.AddComponent<AttackingLogic>();
        enemy = transform.GetComponentInParent<Enemy>();

        NoneState none = new(this);
        PatrollingState patrolling = new(this);
        HidingState hiding = new(this);
        BehindCoverState behindCover = new(this);
        AttackingState attacking = new(this);
        ChasingState chasing = new(this);
        SearchingState searching = new(this);



        stateMashine = new AiStateMashine<AiState>(none, this);

        Dictionary<StateTransition<AiState>, State<AiState>> transitions = new Dictionary<StateTransition<AiState>, State<AiState>>
        {
            { new StateTransition<AiState>(AiState.None, AiState.Patrolling), patrolling },
            { new StateTransition<AiState>(AiState.None, AiState.Searching), searching },


            { new StateTransition<AiState>(AiState.Patrolling, AiState.None), none },
            { new StateTransition<AiState>(AiState.Patrolling, AiState.Hiding), hiding },
            { new StateTransition<AiState>(AiState.Patrolling, AiState.Attacking), attacking },
            { new StateTransition<AiState>(AiState.Patrolling, AiState.Chasing), chasing },


            { new StateTransition<AiState>(AiState.Hiding, AiState.Attacking), attacking },
            { new StateTransition<AiState>(AiState.Hiding, AiState.BehindCover), behindCover },

            { new StateTransition<AiState>(AiState.BehindCover, AiState.Attacking), attacking },
            { new StateTransition<AiState>(AiState.BehindCover, AiState.Chasing), chasing },

            { new StateTransition<AiState>(AiState.Attacking, AiState.Chasing), chasing },
            { new StateTransition<AiState>(AiState.Attacking, AiState.BehindCover), behindCover },

            { new StateTransition<AiState>(AiState.Chasing, AiState.Hiding), hiding },
            { new StateTransition<AiState>(AiState.Chasing, AiState.Attacking), attacking },
            { new StateTransition<AiState>(AiState.Chasing, AiState.Patrolling), patrolling },
            { new StateTransition<AiState>(AiState.Chasing, AiState.Searching), searching },

            { new StateTransition<AiState>(AiState.Searching, AiState.Hiding), hiding },



        };
        stateMashine.SetupTransitions(transitions);

    }
    public Transform GetEyes() {
        return eyes;
    }
    private void Start() {
        if (aiEnabled) {
            //SetPatrollingPath();
            enemy.GetAimPosition().OnLineOfSight += EnemySpotted;
            enemy.OnHealthLoss += HealthLoss;
            aiState = AiState.Patrolling;
            //aiState = AiState.Searching;

        }
    }

    public Gun getWeapon() {
        return enemy.gun;
    }

    public void setSquad(Squad squad) {
        enemy.squad = squad;
    }

    private void EnemySpotted(object sender = null, System.EventArgs e = null) {
        enemy.GetAimPosition().OnLineOfSight -= EnemySpotted;
        Player player = FindFirstObjectByType<Player>();
        //attackingLogic.opponent = player;
        //attackingLogic.EnemySpotted();

        ICommand command = new EnemySpottedCommand(enemy.squad, player);
        command.execute();
    }
    public void EnemySpotted(Character character) {
        enemy.GetAimPosition().OnLineOfSight -= EnemySpotted;
        attackingLogic.opponent = character;
        attackingLogic.EnemySpotted(character);
    }
    public void SetUpdateStateCallback(Action callback) {
        stateMashineCallback = callback;
    }

    private void Update() {
        if (stateMashineCallback != null) stateMashineCallback();
        if (!aiEnabled) return;
        stateMashine.UpdateState();

        Debug.Log(aiState);
    }
    
    public void HealthLoss(object sender, BulletDataEventArgs eventArgs) {
        if (!enemy.GetAimPosition().aimingAtCharacter) {
            enemy.GetAimPosition().LockOnTarget(eventArgs.BulletData.source);
        }
    }

    public Vector3[] SetNewDestinaction(Vector3 spot) {
        currentCorner = 1;
        onFinalPosition = false;
        OnFinalPositionEvent = null;
        OnNextCornerEvent = null;
        navAgent.CalculatePath(spot, destination);
        return destination.corners;
    }
    //public Vector3 GetOpponentLastKnownPosition() {
    //    return AttackingLogic.lastKnownPosition;
    //}


    public void FinalPosition() {
        if (OnFinalPositionEvent != null) OnFinalPositionEvent(this, EventArgs.Empty);
    }
    public void NextCorner() {
        if (OnNextCornerEvent != null) OnNextCornerEvent(this, EventArgs.Empty);
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
        if (OnAimStart != null) OnAimStart(this, EventArgs.Empty);
    }
    public void AimPerformed() {
        if (OnAimPerformed != null) OnAimPerformed(this, EventArgs.Empty);
    }
    public void AimCancel() {
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
    public void Reload() {
        if (OnReloadStart != null) OnReloadStart(this, EventArgs.Empty);
    }
    public void ReloadCancel() {
        if (OnReloadCancel != null) OnReloadCancel(this, EventArgs.Empty);
    }


    //public void SetPatrollingPath(object sender = null, System.EventArgs e = null) {
    //    if (patrolPositions.Length == 0) {
    //        onFinalPosition = true;

    //        return;
    //    };
    //    if ((currentPatrolPosition == -1 || onFinalPosition) && aiState == AiState.Patrolling) {
    //        patrolPointSelection();
    //        if(pathRecalc != null) StopCoroutine(pathRecalc);
    //        pathRecalc = RecalculatePath();
    //        StartCoroutine(pathRecalc);
    //    }
    //}

    //private void patrolPointSelection() {
    //    System.Random random = new System.Random();
    //    currentPatrolPosition = random.Next(0, 3);
    //    SetNewDestinaction(patrolPositions[currentPatrolPosition].transform.position);
    //    OnFinalPositionEvent += SetPatrollingPath;
    //}

    //private IEnumerator RecalculatePath() {
    //    while(!onFinalPosition) {
    //        currentCorner = 1;
    //        navAgent.CalculatePath(destination.corners[destination.corners.Length - 1], destination);
    //        yield return new WaitForSeconds(.5f);
    //    }
    //}

    private void DrawLine() {
        pathLine.positionCount = destination.corners.Length;
        pathLine.SetPosition(0, transform.parent.position);


        for(int i = 1; i< destination.corners.Length; i++) {
            var corner = destination.corners[i];
            pathLine.SetPosition(i, new Vector3(corner.x, corner.y, corner.z));

        }
    }

    public Vector2 GetMovementVectorNormalized() {

        //if (aiState == AiState.Patrolling) SetPatrollingPath();

        if(destination == null || destination.corners.Length == 0 || onFinalPosition) return Vector2.zero;

        if (Vector3.Distance(groundSpot.position, destination.corners[destination.corners.Length - 1]) < 0.2f && onFinalPosition == false) {
            onFinalPosition = true;
            FinalPosition();
            //OnFinalPositionEvent = null;
            return new Vector2(0, 0);
        }
        moveDir = destination.corners[currentCorner] - groundSpot.position;

        if (Vector3.Distance(groundSpot.position, destination.corners[currentCorner]) < .2f && !onFinalPosition) {
            if(destination.corners.Length > 2) {
                moveDir = destination.corners[currentCorner] - groundSpot.position;
                currentCorner++;
                NextCorner();
            }
        }

        return new Vector2(moveDir.x, moveDir.z).normalized;
    }

    private void OnDrawGizmos() {
        foreach (var area in SearchingState.areas) {
            if (area.AreaChecked) {
                Gizmos.color = Color.yellow;
            } else {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawCube(area.Position, Vector3.one);
        }


    }
}