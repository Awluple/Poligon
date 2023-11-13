using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai {

    public abstract class State<T> : IState {

        public abstract T state { get; protected set; }

        public virtual void EnterState() {
        }

        public virtual void ExitState() {

        }

        public virtual void UpdateState() {

        }
    }
}

