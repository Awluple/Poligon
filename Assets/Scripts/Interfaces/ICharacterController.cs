using System;
using UnityEngine;

public interface ICharacterController {
    event EventHandler OnAimCancel;
    event EventHandler OnAimPerformed;
    event EventHandler OnAimStart;
    event EventHandler OnCrouchCancel;
    event EventHandler OnCrouchPerformed;
    event EventHandler OnCrouchStart;
    event EventHandler OnJumpCancel;
    event EventHandler OnJumpPerformed;
    event EventHandler OnJumpStart;
    event EventHandler OnRunCancel;
    event EventHandler OnRunStart;
    event EventHandler OnShootCancel;
    event EventHandler OnShootPerformed;
    event EventHandler OnShootStart;

    Vector2 GetMovementVectorNormalized();
}