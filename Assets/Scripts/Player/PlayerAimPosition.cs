using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAimPosition : AimPosition {

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask mask;


    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, 999f, mask)) {
            Vector3 position = hit.point;
            transform.position = position;
        }
    }
}
