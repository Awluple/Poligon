using System.Collections;
using UnityEngine;
using Poligon.Ai.EnemyStates.Utils;
using System.Collections.Generic;

namespace Poligon.Ai.EnemyStates {
    public class HidingState : EnemyBaseState {
        private IEnumerator movingAttackCoroutine;
        private IEnumerator attemptHideCoroutine;


        public HidingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Hiding;

        public override void EnterState() {
            CoverPosition hidingSpot = enemyController.hidingLogic.currentCoverPosition;

            if (hidingSpot != null && hidingSpot.transform.position != Vector3.zero) {
                enemyController.SetNewDestinaction(hidingSpot.transform.position);
                enemyController.OnFinalPositionEvent += OnPosition;
                enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, true, 0.15f);

            } else {
                enemyController.SetNewDestinaction(enemyController.transform.position);
                enemyController.CrouchStart();
                attemptHideCoroutine = AttemptHideCoroutine();
                enemyController.StartCoroutine(attemptHideCoroutine);
                movingAttackCoroutine = Coroutines.ContinueAttackingWhileMoving(enemyController, false);
            }


            enemyController.StartCoroutine(movingAttackCoroutine);
        }

        private IEnumerator AttemptHideCoroutine() {
            for (; ; ) {
                if (Methods.HasAimOnOpponent(out Character character, enemyController)) {
                    enemyController.hidingLogic.GetHidingPosition(enemyController.attackingLogic.opponent.transform.position);
                } else {
                    enemyController.hidingLogic.GetHidingPosition(enemyController.enemy.GetAimPosition().transform.position);
                }

                CoverPosition hidingSpot = enemyController.hidingLogic.currentCoverPosition;
                if (hidingSpot != null && hidingSpot.transform.position != Vector3.zero) {
                    enemyController.CrouchCancel();

                    enemyController.SetNewDestinaction(hidingSpot.transform.position);
                    enemyController.OnFinalPositionEvent += OnPosition;
                    enemyController.OnFinalPositionEvent += (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

                    enemyController.StopCoroutine(attemptHideCoroutine);
                }


                yield return new WaitForSeconds(3f);
            }
        }

        private void OnPosition(object sender = null, System.EventArgs e = null) {
            SetBehindCoverPosition();
        }
        public override void ExitState() {
            if (movingAttackCoroutine != null) enemyController.StopCoroutine(movingAttackCoroutine);
            if (attemptHideCoroutine != null) enemyController.StopCoroutine(attemptHideCoroutine);
            
            enemyController.OnFinalPositionEvent -= OnPosition;
            enemyController.OnFinalPositionEvent -= (object sender, System.EventArgs e) => { enemyController.RunCancel(); };

            enemyController.ShootCancel();
        }
        public void SetBehindCoverPosition() {

            if (!Methods.HasAimOnOpponent(out Character chara, enemyController)) {
                enemyController.enemy.RotateSelf(-enemyController.hidingLogic.currentCoverPosition.transform.position);
                List<CoverPose> poses = enemyController.hidingLogic.currentCoverPosition.GetCoverPoses();
                if (poses.Count == 1) {

                    if (poses[0] == CoverPose.Standing) {
                        enemyController.CrouchStart();
                    }
                }
                enemyController.aiState = AiState.BehindCover;
                //coveredAttackCoroutine = ContinueAttackingWhileCovered();
                //enemyController.StartCoroutine(coveredAttackCoroutine);
            } else {
                enemyController.aiState = AiState.Attacking;
            }
        }
    }
}