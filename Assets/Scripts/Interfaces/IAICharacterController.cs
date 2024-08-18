using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.Ai;
public interface IAICharacterController : ICharacterController
{
    bool isEnabled();
    void EnemySpotted(Character character);

    void setSquad(Squad squad);
    Character GetCharacter();
    HidingLogic hidingLogic { get; }
    AttackingLogic attackingLogic { get; }
}
