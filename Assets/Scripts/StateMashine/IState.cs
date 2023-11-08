using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    abstract void EnterState();
    abstract void UpdateState();
    abstract void ExitState();

}
