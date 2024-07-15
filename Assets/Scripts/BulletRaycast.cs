using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletRaycast : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float lifetime = 0f;

    public void Setup(BulletData bulletData) {
        Ray ray = new Ray(transform.position, bulletData.targetPosition);
        Physics.Raycast(ray, out RaycastHit hitInfo, 999f);
        
        if(hitInfo.collider != null) {
            targetPosition = hitInfo.point;
        } else {
            targetPosition = ray.GetPoint(300f);
        }
        Debug.DrawRay(transform.position, (targetPosition - transform.position), Color.magenta, 0.4f);

        if(hitInfo.collider != null && hitInfo.collider.gameObject.TryGetComponent(out IKillable component)) {
            component.ApplyDamage(bulletData);
        }

    }
    void Update()
    {
        
        float distanceBefore = Vector3.Distance(transform.position, targetPosition);

        Vector3 moveDir = (targetPosition - transform.position).normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfter = Vector3.Distance(transform.position, targetPosition);

        if(distanceBefore < distanceAfter || lifetime > 2f) {
            Destroy(gameObject);
        }
        lifetime += Time.deltaTime;
    }
}
