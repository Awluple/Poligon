using Poligon.Ai.Commands;
using Poligon.EvetArgs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Poligon.Ai {
    public class Squad : MonoBehaviour {
        public List<IAICharacterController> characters = new ();
        public Dictionary<Character, LastKnownPosition> lastKnownPosition { get; private set; } = new Dictionary<Character, LastKnownPosition>();
        public Dictionary<Character, Character> knownEnemies = new ();
        private float enemySinceLastSeen = 0f;

        void Start() {
            List<IAICharacterController> childrenCharacters = GetComponentsInChildren<IAICharacterController>().ToList();
            foreach (var character in childrenCharacters) {
                if (character.isEnabled()) {
                    characters.Add(character);
                    character.setSquad(this);
                }
            }
        }
        private void Update() {
            enemySinceLastSeen += Time.deltaTime;
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
            return lastKnownPosition.Values.ToList()[0];
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