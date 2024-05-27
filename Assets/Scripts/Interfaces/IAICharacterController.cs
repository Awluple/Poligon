using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAICharacterController : ICharacterController
{
    void EnemySpotted(Character character);

    void setSquad(Squad squad);
    HidingLogic hidingLogic { get; }
    AttackingLogic attackingLogic { get; }
}
