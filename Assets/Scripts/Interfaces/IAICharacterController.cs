using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.Ai;
using Poligon.Ai.States;
public interface IAICharacterController : ICharacterController
{
    bool isEnabled();
    void EnemySpotted(Character character);
    Vector3[] SetNewDestinaction(Vector3 spot, object sender = null);
    void setSquad(Squad squad);
    Character GetCharacter();
    HidingLogic hidingLogic { get; }
    AttackingLogic attackingLogic { get; }
}
