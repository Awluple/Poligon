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
    public virtual void Reposition(Vector3 newPosition) {
        transform.position = newPosition;
    }

}
