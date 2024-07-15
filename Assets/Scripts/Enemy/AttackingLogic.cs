using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Poligon.Ai.Commands;
using Poligon.Ai.EnemyStates;
using Poligon.Ai.EnemyStates.Utils;
using Poligon.EvetArgs;

public class AttackingLogic : MonoBehaviour {

    private EnemyController enemyController;

    //private IEnumerator coveredAttackCoroutine;
    private IEnumerator checkLastSeen;
    private IEnumerator keepTrackOnEnemyCoroutine;

    [SerializeField] private Character _opponent;
    public Character opponent { get { return _opponent; } 
        set { 
            if(_opponent != null) {
                _opponent.OnDeath -= OpponentDeath;
            }
            _opponent = value;
            _opponent.OnDeath += OpponentDeath;
        } }

    public float enemySinceLastSeen { get; private set; }

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }
    private void OpponentDeath(object sender, CharacterEventArgs args) {
        args.character.OnDeath -= OpponentDeath;
    }
    private void UpdateLastKnownPosition(Character character, Vector3 newPosition) {
        enemyController.enemy.squad.UpdateLastKnownPosition(new LastKnownPosition(character, newPosition));
    }

    public void EnemySpotted(Character character) {
        
        if(Methods.HasVisionOnCharacter(out Character hitChar, enemyController, character, 70f)) {
            enemyController.hidingLogic.GetHidingPosition(character.transform.position);
            enemyController.aiState = AiState.Hiding;
            StartTrackCoroutine();
            //Debug.DrawRay(enemyController.eyes.transform.position, character.transform.position - enemyController.eyes.transform.position, Color.magenta, 2f);
        } else {
            enemyController.aiState = AiState.Chasing;
        }

        if (checkLastSeen != null) { StopCoroutine(checkLastSeen); }
        //checkLastSeen = CheckLastSeen();
        //StartCoroutine(checkLastSeen);


    }
    public void StartTrackCoroutine() {
        keepTrackOnEnemyCoroutine = KeepTrackOnEnemy();
        StartCoroutine(keepTrackOnEnemyCoroutine);
    }

    //IEnumerator CheckLastSeen() {
    //    for (; ; ) {
    //        if (Time.time - enemySinceLastSeen > 50f && enemyController.aiState != AiState.Chasing) {
    //            enemyController.aiState = AiState.Chasing;
    //            enemyController.SetNewDestinaction(enemyController.enemy.squad.lastKnownPosition);

    //            enemyController.RunCancel();
    //            enemyController.enemy.GetAimPosition().Reset();
    //        } else if (Time.time - enemySinceLastSeen > 60f) {
    //            //StopAttacking();
    //            enemyController.aiState = AiState.Searching;
    //            StopCoroutine(checkLastSeen);
    //        }
    //        yield return new WaitForSeconds(1f);
    //    }
    //}

    IEnumerator KeepTrackOnEnemy() {
        bool needsReposition = false;
        for (; ; ) {
            Character character = null;
            bool hasVis = Methods.HasAimOnOpponent(out character, enemyController);
            if (hasVis) {
                UpdateLastKnownPosition(character, character.detectionPoints[0].transform.position);
                enemySinceLastSeen = Time.time;
                needsReposition = true;
            } else {
                
                if (needsReposition) enemyController.enemy.GetAimPosition().Reposition(enemyController.enemy.squad.GetCharacterLastPosition(opponent).position);
                needsReposition = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
