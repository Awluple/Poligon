using System;
using System.Collections;
using UnityEngine;


namespace Poligon.Ai.EnemyStates.Utils {

    public static class Coroutines {
        /// <summary>
        /// Continue shooting at target while moving. When no ammo, reload a weapon.
        /// </summary>
        /// <param name="enemyController">The bot's controller</param>
        /// <param name="runWhenNoVision">If there is no vision on an opponent, start running</param>
        /// <param name="timeout">Timeout for attacking at, useful if some other actions needs to take place before (like aim position change)</param>
        public static IEnumerator ContinueAttackingWhileMoving(EnemyController enemyController, bool runWhenNoVision = true, float timeout = 0f) {
            bool shootCalled = false;
            yield return new WaitForSeconds(timeout);
            for (; ; ) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController);
                if (!hasVision) {
                    enemyController.ShootCancel();
                    shootCalled = false;
                }
                if (hasVision && !(enemyController.getWeapon().currentAmmo == 0)) {
                    if (!enemyController.enemy.IsAiming()) enemyController.AimStart();
                    if (character == enemyController.enemy) { yield return new WaitForSeconds(0.3f); } // don't shoot yourself...

                    if (!enemyController.getWeapon().automatic) {
                        enemyController.ShootPerformed();
                    } else if (enemyController.getWeapon().automatic && !shootCalled) {
                        enemyController.ShootPerformed();
                        shootCalled = true;
                    }

                } else {

                    if (runWhenNoVision) {
                        enemyController.AimCancel();
                        enemyController.RunStart();
                        enemyController.ShootCancel();
                        shootCalled = false;
                    }
                }
                if (enemyController.getWeapon().currentAmmo == 0) {
                    shootCalled = false;
                    enemyController.ShootCancel();
                    enemyController.Reload();
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f, 1.2f));
            }
        }
        /// <summary>
        /// Shoot at target. When no ammo, reload a weapon.
        /// </summary>
        /// <param name="enemyController">The bot's controller</param>
        public static IEnumerator ShootingCoroutine(EnemyController enemyController) {
            bool shootCalled = false;
            for (; ; ) {
                bool hasVision = Methods.HasAimOnOpponent(out Character character, enemyController);
                if (!hasVision) enemyController.ShootCancel(); shootCalled = false;
                if(!hasVision && enemyController.enemy.IsCrouching() && enemyController.getWeapon().currentAmmo != 0) enemyController.CrouchCancel() ;

                if (hasVision && !(enemyController.getWeapon().currentAmmo == 0)) {
                    if (character != enemyController.enemy && !enemyController.getWeapon().automatic) {
                        enemyController.ShootPerformed();
                    } else if (character != enemyController.enemy && enemyController.getWeapon().automatic && !shootCalled) {
                        enemyController.ShootPerformed();
                        shootCalled = true;
                    }
                } else if (!hasVision && !(enemyController.getWeapon().currentAmmo == 0)) {
                    shootCalled = false;
                    enemyController.ShootCancel();
                    if (enemyController.enemy.IsCrouching()) {
                        enemyController.CrouchCancel();
                    }
                }

                if (enemyController.getWeapon().currentAmmo == 0 && enemyController.getWeapon().ammoStock != 0) {
                    shootCalled = false;
                    enemyController.ShootCancel();
                    enemyController.Reload();
                    if (!enemyController.enemy.IsCrouching()) {
                        enemyController.CrouchStart();
                        enemyController.OnReloadCancel += Stand;
                    }
                } else if (enemyController.getWeapon().currentAmmo == 0 && enemyController.getWeapon().ammoStock == 0) {
                    enemyController.ShootCancel();
                    enemyController.ChangeWeapon(Enums.WeaponTypes.Pistol);
                    yield return new WaitForSeconds(0.3f);
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
            };
            void Stand(object sender, EventArgs e) {
                enemyController.OnReloadCancel -= Stand;
                enemyController.CrouchCancel();
            }
        }
    }
    public static class Methods {
        /// <summary>
        /// Checks if a bot has vision on it's opponent.
        /// </summary>
        /// <param name="character">The character that has been hit by the raycast</param>
        /// <param name="enemyController">The bot's controller</param>
        /// <param name="maxDistance">Maximum distance to check visibility.</param>
        /// <returns></returns>
        public static bool HasAimOnOpponent(out Character character, EnemyController enemyController, float maxDistance = 40f) {
            if (enemyController.enemy == null || enemyController.enemy.GetAimPosition() == null) { character = null; return false; }
            Vector3 eyesPosition = enemyController.eyes.transform.position;
            Ray ray = new Ray(eyesPosition, enemyController.enemy.GetAimPosition().GetPosition() - eyesPosition);
            int layerMask = (1 << 9) | (1 << 20); // ignore Enemy, Cover and Character masks
            if (enemyController.enemy.team == Enums.Team.Enemy) {
                layerMask |= (1 << 8);
            } else {
                layerMask |= (1 << 11);
            }
            layerMask = ~layerMask;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask)) {
                if (hit.collider.gameObject.TryGetComponent<Character>(out Character hitCharacter)) {
                    character = hitCharacter;
                    if (character.team != enemyController.enemy.team) {
                        return true;
                    }
                }

            }
            character = null;
            return false;
        }
        /// <summary>
        /// Checks if a bot has vision on a diffrent character.
        /// </summary>
        /// <param name="character">The character that has been hit by the raycast</param>
        /// <param name="enemyController">The bot's controller</param>
        /// <param name="target">The targer character</param>
        /// <param name="maxDistance">Maximum distance to check visibility.</param>
        public static bool HasVisionOnCharacter(out Character character, EnemyController enemyController, Character target, float maxDistance = 40f) {
            if(enemyController.enemy == null) { character = null;  return false; }
            Vector3 eyesPosition = enemyController.eyes.transform.position;
            if (target == null) {
                character = null;
                return false;
            }
            foreach (var detectionPoint in target.detectionPoints) {
                Ray ray = new Ray(eyesPosition, detectionPoint.transform.position - eyesPosition);
                int layerMask = (1 << 9) | (1 << 20); // ignore Enemy, Cover and Character masks
                if (enemyController.enemy.team == Enums.Team.Enemy) {
                    layerMask |= (1 << 8);
                } else {
                    layerMask |= (1 << 11);
                }
                layerMask = ~layerMask;
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, ~layerMask)) {
                    if (hit.collider.gameObject.TryGetComponent<Character>(out Character hitCharacter)) {
                        character = hitCharacter;
                        return true;
                    }

                }
            }
            character = null;
            return false;
        }
    }
}