using System.Collections;
using UnityEngine;

namespace Poligon.Ai.EnemyStates.Utils {

    public static class Coroutines {
        public static IEnumerator ContinueAttackingWhileMoving(EnemyController enemyController, bool runWhenNoVision = true) {
            for (; ; ) {
                if (Methods.HasVisionOnOpponent(out Character character, enemyController)) {
                    if (!enemyController.enemy.IsAiming()) enemyController.AimStart();
                    if (character == enemyController.enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...
                    enemyController.ShootPerformed();
                } else {
                    if (runWhenNoVision) {
                        enemyController.AimCancel();
                        enemyController.RunStart();
                    }
                }
                yield return new WaitForSeconds(Random.Range(0.8f, 1.2f));
            }
        }
        public static IEnumerator ShootingCoroutine(EnemyController enemyController) {
            for (; ; ) {
                if (Methods.HasVisionOnOpponent(out Character character, enemyController)) {
                    if (character != enemyController.enemy) enemyController.ShootPerformed();
                }
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            }
        }
    }
    public static class Methods {
        public static bool HasVisionOnOpponent(out Character character, EnemyController enemyController, float maxDistance = 40f) {
             
            Vector3 eyesPosition = enemyController.eyes.transform.position;
            Ray ray = new Ray(eyesPosition, enemyController.enemy.GetAimPosition().transform.position - eyesPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance)) {
                if (hit.collider.gameObject.TryGetComponent<Character>(out Character hitCharacter)) {
                    character = hitCharacter;
                    return true;
                }

            }
            character = null;
            return false;
        }
    }
}