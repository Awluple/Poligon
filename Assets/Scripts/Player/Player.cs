using System;
using UnityEngine;
using Poligon.EvetArgs;
using UnityEngine.AI;

public class Player : Character {


    [SerializeField] private float jumpStrength = 14f;
    [SerializeField] private float jumpGracePeriod = 0.1f;

    private float originalStepOffset;
    private float? lastGroundedTime;
    private bool jumped = false;

    [SerializeField] private GameInput gameInput;

    public event EventHandler OnJumpStart;
    public event EventHandler OnJumpEnd;


    // Start is called before the first frame update
    void Start() {
        gameInput.OnRunStart += StartRun;
        gameInput.OnRunCancel += CancelRun;

        gameInput.OnJumpStart += Jump;

        gameInput.OnAimStart += AimStart;
        gameInput.OnAimCancel += AimCancel;

        gameInput.OnCrouchStart += StartCrouch;
        gameInput.OnCrouchCancel += CancelCrouch;

        gameInput.OnShootPerformed += ShootPerformed;
        gameInput.OnShootCancel += ShootCancel;

        gameInput.OnLeaningStart += LeaningStart;
        gameInput.OnLeaningCancel += LeaningCancel;

        gameInput.OnWeaponChangePerformed += ChangeWeapon;

        gameInput.OnReloadPerformed += Reload;


        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;

    }


    // Update is called once per frame
    protected override void Update() {
        base.Update();

        if(!stunned) {
            Move();
        } else {
            movement.x = 0;
            movement.z = 0;
        }

        movement.y += Physics.gravity.y * fallingGravityStrength * Time.deltaTime;

        if (isWalking == false) {
            isRunning = false;
        }

        if (characterController.isGrounded) {
            lastGroundedTime = Time.time;
        }
        if (characterController.isGrounded && jumped == false) {
            movement.y = -1f;
            characterController.stepOffset = originalStepOffset;
        }

        if ((isWalking && !stunned) || isAiming) {
            Rotate(new Vector3(movement.x, 0f, movement.z));
        }

        characterController.Move(movement * Time.deltaTime);
        int layer_mask = LayerMask.GetMask("Ground");
        bool hit = false;

        if(!characterController.isGrounded) {
            hit = Physics.Raycast(transform.position, Vector3.down, 2f, layer_mask);
        }

        if (hit && movement.y < 0f) {
            if (movement.y <= -25f) {
                stunned = true;
                StartCoroutine(HeavyFallStun());
            }
        } else if(!characterController.isGrounded && !hit && !(movement.y >= 0f)) {
            if (OnFalling != null) OnFalling(this, EventArgs.Empty);
        }

    }
    private void Jump(object sender, System.EventArgs e) {

        if (!stunned && Time.time - lastGroundedTime <= jumpGracePeriod) { // allow jumping in air if touched ground in last 'jumpGracePeriod' seconds
            if (OnJumpStart != null) OnJumpStart(this, EventArgs.Empty);
            characterController.stepOffset = 0;
            movement.y = jumpStrength;
            jumped = true;
        }
    }

    private void LateUpdate() {
        jumped = false;
    }

    protected override void Move() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        isWalking = inputVector.x != 0 || inputVector.y != 0;
        Vector2 aimBlendTree = Vector2.zero;

        if (IsAiming() && !isWalking) {
            if (OnAimingWalk != null) OnAimingWalk(this, new Vector2EventArgs(new Vector2(0, 0)));
        }
        if (IsAiming() && isWalking) {
            aimBlendTree = GetAimWalkBlendTreeValues(inputVector);

            if (OnAimingWalk != null) OnAimingWalk(this, new Vector2EventArgs(aimBlendTree));
        }

        float speed = IsRunning() == true ? sprintSpeed :
                      isAiming == true ? aimSpeed :
                      moveSpeed;

        if (isCrouching) speed = speed * 0.6f;

        if(aimBlendTree != Vector2.zero) {
            if(aimBlendTree.y < -0.35f || Mathf.Abs(aimBlendTree.x) > 0.35f) {
                speed = speed - speed * 0.3f;
            }
        }

        movement.x = inputVector.x * speed;
        movement.z = inputVector.y * speed;

    }



    NavMeshTriangulation navMeshData;
    bool navmeshReady = false;

    //void OnDrawGizmos() {
    //    List<Vector3> positions = new List<Vector3>();

    //    //if (positions.Count == 0) {
    //    //    for (int radius = 4; radius < 60; radius = radius + 4) {
    //    //        float addition = (60 / radius * (radius < 30 ? 3f : 6f));

    //    //        for (float angle = 1; angle < 360; angle = angle + addition) {
    //    //            float radians = (Mathf.PI / 180) * angle;
    //    //            float x = radius * Mathf.Sin(radians);
    //    //            float y = radius * Mathf.Cos(radians);

    //    //            positions.Add(new Vector3(transform.position.x + x, transform.position.y + 0.5f, transform.position.z + y));
    //    //        }
    //    //    }
    //    //}

    //    List<Vector3> points = new List<Vector3>();
    //    if(!navmeshReady) {
    //        navMeshData = NavMesh.CalculateTriangulation();
    //        navmeshReady = true;
    //    }

    //    for (int i = 0; i < navMeshData.indices.Length; i += 3) {
    //        Vector3 p1 = navMeshData.vertices[navMeshData.indices[i]];
    //        Vector3 p2 = navMeshData.vertices[navMeshData.indices[i + 1]];
    //        Vector3 p3 = navMeshData.vertices[navMeshData.indices[i + 2]];

    //        Vector3 triangleCenter =  (p1 + p2 + p3) / 3f;

    //        float a = Vector3.Distance(p1, p2);
    //        float b = Vector3.Distance(p1, p3);
    //        float c = Vector3.Distance(p2, p3);

    //        float circ =  (a + b + c) / 2;

    //        float triangleAreaSize = Mathf.Sqrt(circ * (circ - a) * (circ - b) * (circ - c));

    //        if(Vector3.Distance(transform.position, triangleCenter) > 50f) {
    //            continue;
    //        }

    //        if (Physics.Raycast(triangleCenter, Vector3.down, out RaycastHit hit, Mathf.Infinity, NavMesh.AllAreas)) {
    //            points.Add(hit.point);
    //        }

    //        if(triangleAreaSize < 10) {
    //            continue;
    //        }

    //        if (triangleAreaSize > 100) {
    //            Gizmos.color = new Color(1, 0, 0, 0.5f);
    //        } else {
    //            Gizmos.color = new Color(0, 1, 0, 0.5f);
    //        }

    //        Gizmos.DrawCube(hit.point, new Vector3(1, 1, 1));

    //    }

    //}
}
