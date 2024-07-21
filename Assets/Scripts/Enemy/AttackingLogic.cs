using Poligon.Ai.Commands;
using Poligon.Ai.EnemyStates;
using Poligon.Ai.EnemyStates.Utils;
using Poligon.EvetArgs;
using System;
using System.Collections;
using UnityEngine;

public class AttackingLogic : MonoBehaviour {

    private EnemyController enemyController;

    //private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator keepTrackOnEnemyCoroutine;

    [SerializeField] private Character _opponent;
    public Character opponent {
        get { return _opponent; }
        set {
            if (_opponent != null) {
                _opponent.OnDeath -= OpponentDeath;
            }
            _opponent = value;
            _opponent.OnDeath += OpponentDeath;
        }
    }

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }
    private void Start() {
        enemyController = transform.GetComponentInParent<EnemyController>();
        enemyController.enemy.GetAimPosition().OnLineOfSightLost += VisionLost;
        enemyController.enemy.GetAimPosition().OnLineOfSight += VisionGained;

    }
    private void Update() {
        if (enemyController.enemy.GetAimPosition().aimingAtCharacter) {
            UpdateLastKnownPosition(opponent, enemyController.enemy.GetAimPosition().transform.position);
        }
    }
    private void OpponentDeath(object sender, CharacterEventArgs args) {
        args.character.OnDeath -= OpponentDeath;
    }
    private void UpdateLastKnownPosition(Character character, Vector3 newPosition) {
        enemyController.enemy.squad.UpdateLastKnownPosition(new LastKnownPosition(character, newPosition));
    }

    public void EnemySpotted(Character character) {

        if (Methods.HasVisionOnCharacter(out Character hitChar, enemyController, character, 70f)) {
            enemyController.hidingLogic.GetHidingPosition(character.transform.position);
            enemyController.aiState = AiState.Hiding;
        } else {
            enemyController.aiState = AiState.Chasing;
        }

        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }


    }
    void VisionGained(object sender, EventArgs args) {
    }
    void VisionLost(object sender, EventArgs args) {
    }
}
