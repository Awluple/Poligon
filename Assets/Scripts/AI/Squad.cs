using Poligon.Ai.Commands;
using Poligon.Ai.States;
using Poligon.EvetArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Poligon.Ai {
    public class Squad : MonoBehaviour, IStateManager {
        public List<AiCharacterController> characters = new ();
        public Dictionary<Character, LastKnownPosition> lastKnownPosition { get; private set; } = new Dictionary<Character, LastKnownPosition>();
        public Dictionary<Character, Character> knownEnemies = new ();
        private float enemySinceLastSeen = 0f;
        public Character _leader;
        private Action stateMashineCallback;


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
            stateMashine = new AiStateMashine<SquadAiState>(none, this);
            Dictionary<StateTransition<SquadAiState>, State<SquadAiState>> transitions = new Dictionary<StateTransition<SquadAiState>, State<SquadAiState>>
        {
                { new StateTransition<SquadAiState>(SquadAiState.None, SquadAiState.FollowingState), following },
        };
            stateMashine.SetupTransitions(transitions);
            aiState = SquadAiState.FollowingState;
        }
        private void Update() {
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
        }
        public LastKnownPosition GetCharacterLastPosition(Character character) {
            return lastKnownPosition.GetValueOrDefault(character);
        }
        public LastKnownPosition GetChasingLocation() {
            if(lastKnownPosition.Count > 1 ) {
                return lastKnownPosition.Values.ToList()[0];
            }
            return null;
        }
        public void EnemySpotted(Character chara) {
            if (!knownEnemies.ContainsKey(chara)) {
                knownEnemies.Add(chara, chara);
                chara.OnDeath += RemoveCharacter;
            }
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
    }
}