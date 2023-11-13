using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poligon.Ai { 
    public interface IStateManager {
        void SetUpdateStateCallback(Action callback);
    }
}
