using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletRaycast : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed = 300f;


    // Update is called once per frame
    public void Setup(Vector3 targetPosition) {
        Physics.Raycast(transform.position, targetPosition, out RaycastHit hitInfo, 999f);
        this.targetPosition = hitInfo.point;

        if(hitInfo.collider.gameObject.TryGetComponent(out IKillable component)) {
            component.ApplyDamage(50);
        }

    }
    void Update()
    {
        if (targetPosition == Vector3.zero) { return; }
        
        float distanceBefore = Vector3.Distance(transform.position, targetPosition);

        Vector3 moveDir = (targetPosition - transform.position).normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfter = Vector3.Distance(transform.position, targetPosition);

        if(distanceBefore < distanceAfter) {
            Destroy(gameObject);
        }
    }
}
