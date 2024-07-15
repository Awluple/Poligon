using Poligon.Ai;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Poligon.Ai.Commands;
using System;
using Poligon.EvetArgs;

namespace Poligon.Ai {
    public class Squad : MonoBehaviour {
        public List<IAICharacterController> characters = new List<IAICharacterController>();
        public Dictionary<Character,LastKnownPosition> lastKnownPosition { get; private set; } = new Dictionary<Character, LastKnownPosition>();
        private float enemySinceLastSeen = 0f;

        void Start() {
            characters = GetComponentsInChildren<IAICharacterController>().ToList();
            foreach (var character in characters) {
                character.setSquad(this);
            }
        }
        private void Update() {
            enemySinceLastSeen += Time.deltaTime;
        }

        public void UpdateLastKnownPosition(LastKnownPosition lastKnownPos) {
            if(!lastKnownPosition.ContainsKey(lastKnownPos.character)) {
                lastKnownPos.character.OnDeath += RemoveCharacter;
            }
            lastKnownPosition[lastKnownPos.character] = lastKnownPos;
        }
        private void RemoveCharacter(object sender, CharacterEventArgs e) {
            lastKnownPosition.Remove(e.character);
        }
        public LastKnownPosition GetCharacterLastPosition(Character character) {
            return lastKnownPosition[character];
        }
        public LastKnownPosition GetChasingLocation() {
            return lastKnownPosition.Values.ToList()[0];
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