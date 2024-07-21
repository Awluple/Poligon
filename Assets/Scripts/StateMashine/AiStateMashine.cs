using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai {
    public struct StateTransition<T> {
        readonly T CurrentState;
        readonly T NextState;


        public StateTransition(T currentState, T nextState) {
            CurrentState = currentState;
            NextState = nextState;
        }

        //public override int GetHashCode() {
        //    return 17 + 31 * CurrentState.GetHashCode() + 31 * NextState.GetHashCode();
        //}

        //public override bool Equals(object obj) {
        //    StateTransition<T> other = obj as StateTransition<T>;
        //    return other != null && this.CurrentState == other.CurrentState && this.NextState == other.NextState;
        //}
    }


    public class AiStateMashine<T> 
        {

        Dictionary<StateTransition<T>, State<T>> transitions;
        public State<T> CurrentState { get; private set; }
        IStateManager stateManager;

        public void SetupTransitions(Dictionary<StateTransition<T>, State<T>> transitionsToSetup) {
            transitions = transitionsToSetup;
        }

        public AiStateMashine(State<T> defaultState, IStateManager stateMng) {
            CurrentState = defaultState;
            CurrentState.EnterState();
            stateManager = stateMng;
        }

        public State<T> GetNext(T command) {
            StateTransition<T> transition = new StateTransition<T>(CurrentState.state, command);
            State<T> nextState;
            if (!transitions.TryGetValue(transition, out nextState))
                throw new Exception("Invalid transition: " + CurrentState.state + " -> " + command);

            //Debug.Log("Tansition from: " + CurrentState + " to: " + nextState);
            return nextState;
        }

        public void UpdateState() {
            CurrentState.UpdateState();
        }

        public T MoveNext(T nextState) {
            CurrentState.ExitState();
            CurrentState = GetNext(nextState);
            CurrentState.EnterState();
            stateManager.SetUpdateStateCallback(CurrentState.UpdateState);
            return CurrentState.state;
        }
        public T GetState() {
            return CurrentState.state;
        }
    }
}
