using Poligon.Ai.Commands;
using Poligon.Ai.States;
using Poligon.Ai.States.Utils;
using Poligon.EvetArgs;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AttackingLogic : MonoBehaviour {
    [SerializeField] CharactersSphere charactersSphere;
    private AiCharacterController aiController;

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
        aiController = transform.GetComponentInParent<AiCharacterController>();
    }
    private void Start() {
        aiController = transform.GetComponentInParent<AiCharacterController>();
        aiController.aiCharacter.GetAimPosition().OnLineOfSightLost += VisionLost;
        aiController.aiCharacter.GetAimPosition().OnLineOfSight += VisionGained;

    }

    private Character GetOpponent() {
        if(this == null) {
            return null;
        }
        var enemies = aiController.aiCharacter.squad.knownEnemies.Values.ToList();
        var validEnemies = enemies.Where(e => e != null && e.gameObject != null).ToList();
        if (validEnemies.Count > 0) {
            // Try to get visible character
            foreach (var character in validEnemies) {
                if (Methods.HasVisionOnCharacter(out Character chara, aiController, character)) {
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
        if (aiController.aiCharacter.GetAimPosition().aimingAtCharacter && opponent != null) {
            if(aiController.aiCharacter.GetAimPosition() != null) UpdateLastKnownPosition(opponent, aiController.aiCharacter.GetAimPosition().transform.position);
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
        aiController.aiCharacter.squad.UpdateLastKnownPosition(new LastKnownPosition(character, newPosition));
    }
    public void CallCharacter(Character opponent) {
        bool hasVision = Methods.HasVisionOnCharacter(out Character hitChar, aiController, opponent, 70f);
        if (hasVision && aiController.aiState != IndividualAiState.BehindCover && aiController.aiState != IndividualAiState.StationaryAttacking) {
            if(aiController.aiState != IndividualAiState.Hiding) aiController.aiState = IndividualAiState.Hiding;
        } else if (aiController.aiState != IndividualAiState.StationaryAttacking
            && aiController.aiState != IndividualAiState.BehindCover && aiController.aiState != IndividualAiState.Chasing
            ) {
            this.opponent = opponent;
            Debug.Log("C " + aiController.aiCharacter.name);
            aiController.aiState = IndividualAiState.Chasing;
        }
    }

    public void EnemySpotted(Character character) {
        bool hasVisionOnCharacter = Methods.HasVisionOnCharacter(out Character hitChar, aiController, character, 70f);
        bool hasVisionOnOpponent = Methods.HasAimOnOpponent(out Character opponentChar, aiController, 70f);
        if (hasVisionOnCharacter && Vector3.Distance(transform.position, opponent.transform.position) < Vector3.Distance(transform.position, hitChar.transform.position)) {
            opponent = hitChar;
        }
        if (!hasVisionOnCharacter && !hasVisionOnCharacter && aiController.aiState != IndividualAiState.Chasing && aiController.aiCharacter.squad.CanChase(aiController)) {
            aiController.aiCharacter.GetAimPosition().LockOnTarget(opponent, !aiController.aiCharacter.IsAiming());
            Debug.Log("B " + aiController.aiCharacter.name);
            if (aiController.aiState != IndividualAiState.Chasing) aiController.aiState = IndividualAiState.Chasing;
        //}
        //else if (hasVisionOnCharacter && aiController.aiState == IndividualAiState.Patrolling) {
        //    aiController.aiState = IndividualAiState.Hiding;
        } else if(aiController.aiState != IndividualAiState.Hiding) {
            aiController.aiState = IndividualAiState.Hiding;
        }
    }
    /// <summary>
    /// Change the current opponent
    /// </summary>
    /// <param name="chase">If the character has no vision on an enemy, go to it's position</param>
    void ChangeOpponent(bool chase = false) {
        opponent = GetOpponent();
        if (opponent != null) {
            aiController.aiCharacter.GetAimPosition().LockOnTarget(opponent, !aiController.aiCharacter.IsAiming());
            bool hasVision = Methods.HasVisionOnCharacter(out Character newOpponent, aiController, opponent);
            if (hasVision && aiController.aiState == IndividualAiState.BehindCover) {
                aiController.aiState = IndividualAiState.StationaryAttacking;
            } else if (aiController.aiState != IndividualAiState.Hiding && hasVision) {
                aiController.aiState = IndividualAiState.Hiding;
            } else if (aiController.aiState != IndividualAiState.Chasing && !hasVision && chase) {
                Debug.Log("A " + aiController.aiCharacter.name);
                aiController.aiState = IndividualAiState.Chasing;
            } else {

            }
        }
    }

    void VisionGained(object sender, EventArgs args) {
        if (!Methods.HasAimOnOpponent(out Character character, aiController) && timeSinceLastOpponentChange > 4f) {
            ChangeOpponent();
        }
    }
    void VisionLost(object sender, EventArgs args) {
    }
}
