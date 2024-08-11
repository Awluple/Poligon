using Poligon.Ai.Commands;
using Poligon.Ai.EnemyStates;
using Poligon.Ai.EnemyStates.Utils;
using Poligon.EvetArgs;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AttackingLogic : MonoBehaviour {
    [SerializeField] CharactersSphere charactersSphere;
    private EnemyController enemyController;

    //private IEnumerator coveredAttackCoroutine;
    private IEnumerator keepTrackOnEnemyCoroutine;

    [SerializeField] private Character _opponent;
    public Character opponent {
        get {
            if (_opponent != null) {
                return _opponent;
            } else {
                var enemies = enemyController.enemy.squad.knownEnemies.Values.ToList();
                if (enemies.Count > 0) {
                    // Try to get visible character
                    foreach (var character in enemies) {
                        if (Methods.HasVisionOnCharacter(out Character chara, enemyController, character)) {
                            opponent = character;
                            return character;
                        }
                    }
                    // Get closest character
                    enemies.Sort((obj1, obj2) =>
                    {
                        float distanceToObj1 = Vector3.Distance(transform.position, obj1.transform.position);
                        float distanceToObj2 = Vector3.Distance(transform.position, obj2.transform.position);

                        return distanceToObj1.CompareTo(distanceToObj2);
                    });
                    return enemies.First();
                }
                
                    
                return null;
            }
        
        }
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
        if (enemyController.enemy.GetAimPosition().aimingAtCharacter && opponent != null) {
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
        bool hasVision = Methods.HasVisionOnCharacter(out Character hitChar, enemyController, character, 70f);
        if (hasVision && Vector3.Distance(transform.position, opponent.transform.position) < Vector3.Distance(transform.position, hitChar.transform.position)) {
            opponent = hitChar;
        }
        if (!hasVision) {
            enemyController.aiState = AiState.Chasing;
        } else if (hasVision && enemyController.aiState == AiState.Attacking) {
            enemyController.hidingLogic.GetHidingPosition(character.transform.position);
            enemyController.aiState = AiState.Hiding;
        } else if(hasVision && enemyController.aiState == AiState.Hiding) {

        }


    }
    void VisionGained(object sender, EventArgs args) {
    }
    void VisionLost(object sender, EventArgs args) {
    }
}
