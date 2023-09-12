using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AimPosition : MonoBehaviour
{
    public Vector3 GetPosition() {
        return transform.position;
    }
    public Transform GetTransform() {
        return transform;
    }

}
