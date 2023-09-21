using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineComponentBase componentBase;
    private float cameraDistance;
    [SerializeField] private float sensitivity = 10f;

    [Range(0, 500)]
    [SerializeField] private float rotationSensitivity = 200f;


    private float rotation = 0;

    public float GetCameraRotation() {
        return virtualCamera.transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(componentBase == null) {
            componentBase = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }
        
        if(Input.GetAxis("Mouse ScrollWheel") != 0) {
            cameraDistance = Input.GetAxis("Mouse ScrollWheel") * sensitivity;

            if(componentBase is CinemachineFramingTransposer) {
                (componentBase as CinemachineFramingTransposer).m_CameraDistance -= cameraDistance;
            }
        }

        if (Input.GetKey(KeyCode.T)) {
            rotation += rotationSensitivity * Time.deltaTime;
            if (componentBase is CinemachineFramingTransposer) {
                Vector3 newRotation = virtualCamera.transform.rotation.eulerAngles;
                newRotation.y = rotation;
                virtualCamera.transform.rotation = Quaternion.Euler(newRotation);
            }
        }
        if (Input.GetKey(KeyCode.Y)) {
            rotation -= rotationSensitivity * Time.deltaTime;
            if (componentBase is CinemachineFramingTransposer) {
                Vector3 newRotation = virtualCamera.transform.rotation.eulerAngles;
                newRotation.y = rotation;
                virtualCamera.transform.rotation = Quaternion.Euler(newRotation);
            }
        }
    }
}
