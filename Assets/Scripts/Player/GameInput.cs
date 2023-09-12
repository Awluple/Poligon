using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour, ICharacterController {
    private PlayerInputActions playerInputActions;
    public event EventHandler OnRunStart;
    public event EventHandler OnRunCancel;

    public event EventHandler OnJumpStart;
    public event EventHandler OnJumpPerformed;
    public event EventHandler OnJumpCancel;

    public event EventHandler OnAimStart;
    public event EventHandler OnAimPerformed;
    public event EventHandler OnAimCancel;

    public event EventHandler OnShootStart;
    public event EventHandler OnShootPerformed;
    public event EventHandler OnShootCancel;

    CameraControl camera;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Run.performed += RunStart;
        playerInputActions.Player.Run.canceled += RunCancel;

        playerInputActions.Player.Jump.started += JumpStart;
        playerInputActions.Player.Jump.performed += JumpPerformed;
        playerInputActions.Player.Jump.canceled += JumpCancel;

        playerInputActions.Player.Aim.started += AimStart;
        playerInputActions.Player.Aim.canceled += AimPerformed;
        playerInputActions.Player.Aim.canceled += AimCancel;

        playerInputActions.Player.Shoot.started += ShootStart;
        playerInputActions.Player.Shoot.canceled += ShootPerformed;
        playerInputActions.Player.Shoot.canceled += ShootCancel;

        camera = FindObjectOfType<CameraControl>();
    }

    private void RunStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnRunStart != null) OnRunStart(this, EventArgs.Empty);
    }
    private void RunCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnRunCancel != null) OnRunCancel(this, EventArgs.Empty);
    }

    private void JumpStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnJumpStart != null) OnJumpStart(this, EventArgs.Empty);
    }
    private void JumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnJumpPerformed != null) OnJumpPerformed(this, EventArgs.Empty);
    }
    private void JumpCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnJumpCancel != null) OnJumpCancel(this, EventArgs.Empty);
    }
    private void AimStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnAimStart != null) OnAimStart(this, EventArgs.Empty);
    }
    private void AimPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnAimPerformed != null) OnAimPerformed(this, EventArgs.Empty);
    }
    private void AimCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnAimCancel != null) OnAimCancel(this, EventArgs.Empty);
    }

    private void ShootStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnShootStart != null) OnShootStart(this, EventArgs.Empty);
    }
    private void ShootPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnShootPerformed != null) OnShootPerformed(this, EventArgs.Empty);
    }
    private void ShootCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnShootCancel != null) OnShootCancel(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        Vector3 tempVector3 = new Vector3(inputVector.x, inputVector.y, 0.0f);

        Quaternion rotation = Quaternion.Euler(0, 0, -camera.GetCameraRotation());
        Vector3 rotatedVector3 = rotation * tempVector3;

        Vector2 rotatedVector2 = new Vector2(rotatedVector3.x, rotatedVector3.y);

        inputVector = rotatedVector2.normalized;

        return inputVector;
    }
}