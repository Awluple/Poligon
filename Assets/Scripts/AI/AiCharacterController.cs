using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Poligon.Ai;
using Poligon.Ai.States;

using Poligon.Ai.Commands;
using Poligon.EvetArgs;
using Poligon.Enums;
using System.Linq;

public class AiCharacterController : MonoBehaviour, IAICharacterController, IStateManager {
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

    public event EventHandler<InputValueEventArgs> OnChangeWeapon;

    public event EventHandler<BulletDataEventArgs> OnHealthLoss;

    public AICharacter aiCharacter { get; private set; }

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
    [SerializeField] private IndividualAiState _debugAiState;
    public IndividualAiState aiState { get => stateMashine.GetState(); set {
            stateMashine.MoveNext(value);
            _debugAiState = value;
        } }
    private AiStateMashine<IndividualAiState> stateMashine;
    private Action stateMashineCallback;

    public HidingLogic hidingLogic { get; private set; }
    public AttackingLogic attackingLogic { get; private set; }
    [SerializeField] bool stopped = false;
    void Awake() {
        navAgent = gameObject.transform.parent.GetComponent<NavMeshAgent>();
        destination = new NavMeshPath();
        hidingLogic = transform.GetComponent<HidingLogic>();
        attackingLogic = transform.gameObject.GetComponent<AttackingLogic>();
        aiCharacter = transform.GetComponentInParent<AICharacter>();

        NoneState none = new(this);
        PatrollingState patrolling = new(this);
        HidingState hiding = new(this);
        BehindCoverState behindCover = new(this);
        StationaryAttackState attacking = new(this);
        ChasingState chasing = new(this);
        SearchingState searching = new(this);
        navAgent.angularSpeed = 780f;
        navAgent.stoppingDistance = 0.4f;

        stateMashine = new AiStateMashine<IndividualAiState>(none, this);

        Dictionary<StateTransition<IndividualAiState>, State<IndividualAiState>> transitions = new Dictionary<StateTransition<IndividualAiState>, State<IndividualAiState>>
        {
            { new StateTransition<IndividualAiState>(IndividualAiState.None, IndividualAiState.Patrolling), patrolling },
            { new StateTransition<IndividualAiState>(IndividualAiState.None, IndividualAiState.Searching), searching },


            { new StateTransition<IndividualAiState>(IndividualAiState.Patrolling, IndividualAiState.None), none },
            { new StateTransition<IndividualAiState>(IndividualAiState.Patrolling, IndividualAiState.Hiding), hiding },
            { new StateTransition<IndividualAiState>(IndividualAiState.Patrolling, IndividualAiState.StationaryAttacking), attacking },
            { new StateTransition<IndividualAiState>(IndividualAiState.Patrolling, IndividualAiState.Chasing), chasing },


            { new StateTransition<IndividualAiState>(IndividualAiState.Hiding, IndividualAiState.StationaryAttacking), attacking },
            { new StateTransition<IndividualAiState>(IndividualAiState.Hiding, IndividualAiState.BehindCover), behindCover },
            { new StateTransition<IndividualAiState>(IndividualAiState.Hiding, IndividualAiState.Chasing), chasing },


            { new StateTransition<IndividualAiState>(IndividualAiState.BehindCover, IndividualAiState.StationaryAttacking), attacking },
            { new StateTransition<IndividualAiState>(IndividualAiState.BehindCover, IndividualAiState.Chasing), chasing },

            { new StateTransition<IndividualAiState>(IndividualAiState.StationaryAttacking, IndividualAiState.Chasing), chasing },
            { new StateTransition<IndividualAiState>(IndividualAiState.StationaryAttacking, IndividualAiState.Hiding), hiding },
            { new StateTransition<IndividualAiState>(IndividualAiState.StationaryAttacking, IndividualAiState.BehindCover), behindCover },

            { new StateTransition<IndividualAiState>(IndividualAiState.Chasing, IndividualAiState.Hiding), hiding },
            { new StateTransition<IndividualAiState>(IndividualAiState.Chasing, IndividualAiState.StationaryAttacking), attacking },
            { new StateTransition<IndividualAiState>(IndividualAiState.Chasing, IndividualAiState.Patrolling), patrolling },
            { new StateTransition<IndividualAiState>(IndividualAiState.Chasing, IndividualAiState.Searching), searching },

            { new StateTransition<IndividualAiState>(IndividualAiState.Searching, IndividualAiState.Hiding), hiding },
            { new StateTransition<IndividualAiState>(IndividualAiState.Searching, IndividualAiState.Chasing), chasing },



        };
        stateMashine.SetupTransitions(transitions);

    }
    public Transform GetEyes() {
        return eyes;
    }
    private void Start() {
        if (!aiEnabled) return;


        aiCharacter.GetAimPosition().OnLineOfSight += EnemySpotted;
        aiCharacter.OnHealthLoss += HealthLoss;
        aiState = IndividualAiState.Patrolling;

        aiCharacter.OnDeath += (object e, CharacterEventArgs ch) => { StopAllCoroutines(); };
    }

    public Gun getWeapon() {
        return aiCharacter.gun;
    }

    public Character GetCharacter() {
        return aiCharacter;
    }

    public void setSquad(Squad squad) {
        aiCharacter.squad = squad;
    }

    private void EnemySpotted(object sender = null, CharacterEventArgs e = null) {
        
        ICommand command = new EnemySpottedCommand(aiCharacter.squad, e.character);
        command.execute();
    }
    public void EnemySpotted(Character character) {
        
        attackingLogic.opponent = character;
        aiCharacter.GetAimPosition().alerted = true;
        attackingLogic.EnemySpotted(character);
    }
    public void SetUpdateStateCallback(Action callback) {
        stateMashineCallback = callback;
    }

    private void Update() {
        if (stateMashineCallback != null) stateMashineCallback();
        if (!aiEnabled) return;
        stateMashine.UpdateState();

        stopped = navAgent.isStopped;
    }
    
    public void HealthLoss(object sender, BulletDataEventArgs eventArgs) {
        if (OnHealthLoss != null) OnHealthLoss(this, eventArgs);
        if (aiCharacter.GetAimPosition() != null && !aiCharacter.GetAimPosition().aimingAtCharacter) {
            aiCharacter.GetAimPosition().LockOnTarget(eventArgs.BulletData.source, !aiCharacter.IsAiming()); // Move the aim to the attacker.
        }
        ICommand command = new CharacterAttackedCommand(aiCharacter,aiCharacter.squad, eventArgs.BulletData.source);
        command.execute();
    }

    public Vector3[] SetNewDestinaction(Vector3 spot, object sender = null) {
        currentCorner = 1;
        onFinalPosition = false;
        OnFinalPositionEvent = null;
        OnNextCornerEvent = null;
        navAgent.CalculatePath(spot, destination);
        if (sender != null) {
            Debug.Log("Sender: " + sender + "| Corners: " + destination.corners.Count());
        }
        navAgent.SetDestination(spot);
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
        if (aiCharacter.IsCrouching()) return;
        if (OnCrouchStart != null) OnCrouchStart(this, EventArgs.Empty);
    }
    public void CrouchPerformed() {
        if (OnCrouchPerformed != null) OnCrouchPerformed(this, EventArgs.Empty);
    }
    public void CrouchCancel() {
        if (!aiCharacter.IsCrouching()) return;
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
    public void ChangeWeapon(WeaponTypes weapon) {
        if (OnChangeWeapon != null) OnChangeWeapon(this, new InputValueEventArgs((int)weapon));
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

        //if (aiState == AiState.Patrolling) SetPatrollingPath();

        if(destination == null || destination.corners.Length == 0 || onFinalPosition) return Vector2.zero;
        //Debug.Log(navAgent.remainingDistance);
        //if (Vector3.Distance(groundSpot.position, destination.corners[destination.corners.Length - 1]) < 0.3f && onFinalPosition == false) {
        //    onFinalPosition = true;
        //    FinalPosition();
        //    navAgent.isStopped = true;
        //    return new Vector2(0, 0);
        //}
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance) {
            onFinalPosition = true;
            FinalPosition();
            navAgent.isStopped = true;
            return new Vector2(0, 0);
        }

        //if (Vector3.Distance(groundSpot.position, destination.corners[destination.corners.Length - 1]) < 0.3f && onFinalPosition == false) {
        //    onFinalPosition = true;
        //    FinalPosition();
        //    navAgent.isStopped = true;
        //    return new Vector2(0, 0);
        //}

        try {
            if(destination.corners.Length == 1) {
                currentCorner = 0;
            }
            moveDir = destination.corners[currentCorner] - groundSpot.position;
        } catch (Exception e) {
            Debug.LogError($"Current corner: {currentCorner} | Corners Count: {destination.corners.Length} | {destination.corners[0]} - {groundSpot.position}");
            throw;
        }

        if (Vector3.Distance(groundSpot.position, destination.corners[currentCorner]) < .35f && !onFinalPosition) {
            if(destination.corners.Length > 2) {
                moveDir = destination.corners[currentCorner] - groundSpot.position;
                currentCorner++;
                NextCorner();
            }
        }
        navAgent.isStopped = false;
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

    public bool isEnabled() {
        return aiEnabled;
    }
}