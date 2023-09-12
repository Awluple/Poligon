using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskObject : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject[] maskObj;
    void Start()
    {
        for(int i = 0; i < maskObj.Length; i++) {
            maskObj[i].GetComponent<MeshRenderer>().material.renderQueue = 3002;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
