using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Poligon.EvetArgs;

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

    public event EventHandler OnCrouchStart;
    public event EventHandler OnCrouchPerformed;
    public event EventHandler OnCrouchCancel;

    public event EventHandler OnShootStart;
    public event EventHandler OnShootPerformed;
    public event EventHandler OnShootCancel;

    public event EventHandler<InputValueEventArgs> OnLeaningStart;
    public event EventHandler<InputValueEventArgs> OnLeaningPerformed;
    public event EventHandler<InputValueEventArgs> OnLeaningCancel;

    CameraControl camera;
    public Transform eyes;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Run.performed += RunStart;
        playerInputActions.Player.Run.canceled += RunCancel;

        playerInputActions.Player.Jump.started += JumpStart;
        playerInputActions.Player.Jump.performed += JumpPerformed;
        playerInputActions.Player.Jump.canceled += JumpCancel;

        playerInputActions.Player.Aim.started += AimStart;
        playerInputActions.Player.Aim.performed += AimPerformed;
        playerInputActions.Player.Aim.canceled += AimCancel;

        playerInputActions.Player.Crouch.started += CrouchStart;
        playerInputActions.Player.Crouch.performed += CrouchPerformed;
        playerInputActions.Player.Crouch.canceled += CrouchCancel;

        playerInputActions.Player.Shoot.started += ShootStart;
        playerInputActions.Player.Shoot.performed += ShootPerformed;
        playerInputActions.Player.Shoot.canceled += ShootCancel;

        playerInputActions.Player.Leaning.started += LeaningStart;
        playerInputActions.Player.Leaning.performed += LeaningPerformed;
        playerInputActions.Player.Leaning.canceled += LeaningCancel;


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
    private void CrouchStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnCrouchStart != null) OnCrouchStart(this, EventArgs.Empty);
    }
    private void CrouchPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnCrouchPerformed != null) OnCrouchPerformed(this, EventArgs.Empty);
    }
    private void CrouchCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnCrouchCancel != null) OnCrouchCancel(this, EventArgs.Empty);
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

    private void LeaningStart(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnLeaningStart != null) OnLeaningStart(this, new InputValueEventArgs(obj.ReadValue<float>()));
    }
    private void LeaningPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnLeaningPerformed != null) OnLeaningPerformed(this, new InputValueEventArgs(obj.ReadValue<float>()));
    }
    private void LeaningCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (OnLeaningCancel != null) OnLeaningCancel(this, new InputValueEventArgs(obj.ReadValue<float>()));
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