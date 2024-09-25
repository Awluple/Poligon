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
            //if (_opponent != null) {
            //    return _opponent;
            //} else {
            //    opponent = GetOpponent();
            //    return _opponent;
            //}
            return _opponent;
        }
        set {
            if (_opponent != null) {
                _opponent.OnDeath -= OpponentDeath;
            }
            _opponent = value;
            if (_opponent != null) {
                _opponent.OnDeath += OpponentDeath;
            }
            timeSinceLastOpponentChange = 0;
        }
    }
    float timeSinceLastOpponentChange = 0f;

    private void Awake() {
        enemyController = transform.GetComponentInParent<EnemyController>();
    }
    private void Start() {
        enemyController = transform.GetComponentInParent<EnemyController>();
        enemyController.enemy.GetAimPosition().OnLineOfSightLost += VisionLost;
        enemyController.enemy.GetAimPosition().OnLineOfSight += VisionGained;

    }

    private Character GetOpponent() {
        if(this == null) {
            return null;
        }
        var enemies = enemyController.enemy.squad.knownEnemies.Values.ToList();
        var validEnemies = enemies.Where(e => e != null && e.gameObject != null).ToList();
        if (validEnemies.Count > 0) {
            // Try to get visible character
            foreach (var character in validEnemies) {
                if (Methods.HasVisionOnCharacter(out Character chara, enemyController, character)) {
                    opponent = character;
                    return character;
                }
            }
            // Get closest character
            validEnemies.Sort((obj1, obj2) => {
                if (obj1 == null || obj1.gameObject == null) return 1;
                if (obj2 == null || obj2.gameObject == null) return -1;
                float distanceToObj1 = Vector3.Distance(transform.position, obj1.transform.position);
                float distanceToObj2 = Vector3.Distance(transform.position, obj2.transform.position);

                return distanceToObj1.CompareTo(distanceToObj2);
            });
            return validEnemies.FirstOrDefault();
        }
        return null;
    }

    private void Update() {
        if (enemyController.enemy.GetAimPosition().aimingAtCharacter && opponent != null) {
            if(enemyController.enemy.GetAimPosition() != null) UpdateLastKnownPosition(opponent, enemyController.enemy.GetAimPosition().transform.position);
        }
        timeSinceLastOpponentChange += Time.deltaTime;
    }
    private void OpponentDeath(object sender, CharacterEventArgs args) {
        args.character.OnDeath -= OpponentDeath;
        //if (enemyController.aiState == AiState.Chasing || enemyController.aiState == AiState.Searching) {
        //    ChangeOpponent();
        //}
        ChangeOpponent();
    }
    private void UpdateLastKnownPosition(Character character, Vector3 newPosition) {
        enemyController.enemy.squad.UpdateLastKnownPosition(new LastKnownPosition(character, newPosition));
    }
    public void CallCharacter(Character opponent) {
        if(enemyController.aiState != AiState.StationaryAttacking && enemyController.aiState != AiState.StationaryAttacking
            && enemyController.aiState != AiState.BehindCover && enemyController.aiState != AiState.Chasing
            ) {
            this.opponent = opponent;
            enemyController.aiState = AiState.Chasing;
        }
    }

    public void EnemySpotted(Character character) {
        bool hasVision = Methods.HasVisionOnCharacter(out Character hitChar, enemyController, character, 70f);
        if (hasVision && Vector3.Distance(transform.position, opponent.transform.position) < Vector3.Distance(transform.position, hitChar.transform.position)) {
            opponent = hitChar;
        }
        if (!hasVision && enemyController.aiState != AiState.Chasing) {
            enemyController.enemy.GetAimPosition().LockOnTarget(opponent, !enemyController.enemy.IsAiming());
            if(enemyController.aiState != AiState.Chasing) enemyController.aiState = AiState.Chasing;
        } else if (hasVision && enemyController.aiState == AiState.StationaryAttacking) {
            //enemyController.hidingLogic.GetHidingPosition(character.transform.position, character);
            enemyController.aiState = AiState.Hiding;
        } else if (hasVision && enemyController.aiState == AiState.Hiding) {

        }
    }

    void ChangeOpponent() {
        opponent = GetOpponent();
        if (opponent != null) {
            enemyController.enemy.GetAimPosition().LockOnTarget(opponent, !enemyController.enemy.IsAiming());
            bool hasVision = Methods.HasVisionOnCharacter(out Character newOpponent, enemyController, opponent);
            if (enemyController.aiState != AiState.Chasing && !hasVision) {
                enemyController.aiState = AiState.Chasing;
            } else if(hasVision && enemyController.aiState == AiState.BehindCover) {
                enemyController.aiState = AiState.StationaryAttacking;
            }
            else if (enemyController.aiState != AiState.Hiding) {
                enemyController.aiState = AiState.Hiding;
            }
        } else {
        }
    }

    void VisionGained(object sender, EventArgs args) {
        if (!Methods.HasAimOnOpponent(out Character character, enemyController) && timeSinceLastOpponentChange > 4f) {
            ChangeOpponent();
        }
    }
    void VisionLost(object sender, EventArgs args) {
    }
}
