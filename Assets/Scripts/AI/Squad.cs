using Poligon.Ai.Commands;
using Poligon.Ai.States;
using Poligon.EvetArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.EventSystems.EventTrigger;

namespace Poligon.Ai {
    public class Squad : MonoBehaviour, IStateManager {
        public List<AiCharacterController> characters = new ();
        public Dictionary<Character, LastKnownPosition> lastKnownPosition { get; private set; } = new Dictionary<Character, LastKnownPosition>();
        public Dictionary<Character, Character> knownEnemies = new ();
        private float enemySinceLastSeen = 0f;
        public Character _leader;
        private Action stateMashineCallback;
        private LastKnownPosition lastEnemyLocation;


        [SerializeField] private SquadAiState _debugAiState;
        public SquadAiState aiState {
            get => stateMashine.GetState(); set {
                stateMashine.MoveNext(value);
                _debugAiState = value;
            }
        }
        private AiStateMashine<SquadAiState> stateMashine;

        public Character leader { get {
                return _leader;
            } set { 
                _leader = value;
                _leader.OnDeath += ReassignLeader;
            } }

        private void ReassignLeader(object sender, CharacterEventArgs ch) {
            if (leader == ch.character) {
                foreach (AiCharacterController teamMember in characters) {
                    if (teamMember != ch.character) { 
                        leader = teamMember.GetCharacter();
                        break;
                    }
                }
            }
            if (leader == null || leader == ch.character) {
                Debug.Log("Squad destroyed");
            }
        }
        public void SetUpdateStateCallback(Action callback) {
            stateMashineCallback = callback;
        }

        void Start() {
            List<AiCharacterController> childrenCharacters = GetComponentsInChildren<AiCharacterController>().ToList();
            foreach (var character in childrenCharacters) {
                if (character.isEnabled()) {
                    characters.Add(character);
                    character.setSquad(this);
                    character.GetCharacter().OnDeath += (object e, CharacterEventArgs args) => { characters.Remove(character); };
                }
            }
            SquadNoneState none = new(this);
            FollowingState following = new(this);
            EngagedState engaged = new(this);
            stateMashine = new AiStateMashine<SquadAiState>(none, this);
            Dictionary<StateTransition<SquadAiState>, State<SquadAiState>> transitions = new Dictionary<StateTransition<SquadAiState>, State<SquadAiState>>
        {
                { new StateTransition<SquadAiState>(SquadAiState.None, SquadAiState.FollowingState), following },

                { new StateTransition<SquadAiState>(SquadAiState.FollowingState, SquadAiState.EngagedState), engaged },
                { new StateTransition<SquadAiState>(SquadAiState.FollowingState, SquadAiState.None), none },

                { new StateTransition<SquadAiState>(SquadAiState.EngagedState, SquadAiState.FollowingState), following },
                { new StateTransition<SquadAiState>(SquadAiState.EngagedState, SquadAiState.None), none },
        };
            stateMashine.SetupTransitions(transitions);
            aiState = SquadAiState.FollowingState;
        }
        private void Update() {
            if (characters.Count == 0) {
                SquadNoneState none = new(this);
                stateMashine = new AiStateMashine<SquadAiState>(none, this);
                StopAllCoroutines();
                return;
            }
            if (stateMashineCallback != null) stateMashineCallback();
            enemySinceLastSeen += Time.deltaTime;
            stateMashine.UpdateState();
        }

        public void UpdateLastKnownPosition(LastKnownPosition lastKnownPos) {
            if (!lastKnownPosition.ContainsKey(lastKnownPos.character)) {
                lastKnownPos.character.OnDeath += RemoveCharacter;
            }
            lastKnownPosition[lastKnownPos.character] = lastKnownPos;
        }
        private void RemoveCharacter(object sender, CharacterEventArgs e) {
            lastKnownPosition.Remove(e.character);
            knownEnemies.Remove(e.character);
            e.character.OnDeath -= RemoveCharacter;
            if (knownEnemies.Count == 0) {
                lastEnemyLocation = new LastKnownPosition(e.character, e.character.transform.position);
            }
        }
        public LastKnownPosition GetCharacterLastPosition(Character character) {
            if(character == null) return null;
            return lastKnownPosition.GetValueOrDefault(character);
        }
        public LastKnownPosition GetChasingLocation() {
            if(lastKnownPosition.Count > 1 ) {
                return lastKnownPosition.Values.ToList()[0];
            }
            if (lastEnemyLocation != null) return lastEnemyLocation;
            return null;
        }

        public void EnemySpotted(Character chara) {
            if(chara == null) {
                throw new ArgumentNullException("Spotted character cannot be null!");
            }
            if (!knownEnemies.ContainsKey(chara) && chara.health > 0) {
                knownEnemies.Add(chara, chara);
                chara.OnDeath += RemoveCharacter;
                lastEnemyLocation = null;
            }
        }

        public bool CanChase(AiCharacterController characterController) {
            int chasingCharacters = characters.Count(character => character.aiState == IndividualAiState.Chasing);

            if (characters.Count > 2 && chasingCharacters < 2) return true;
            if (characters.Count <= 2 && chasingCharacters == 0) return true;
            return false;
        }
    }
}