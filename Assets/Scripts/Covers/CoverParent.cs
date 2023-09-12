using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CoverParent : MonoBehaviour
{
    [SerializeField]CoverObject cover;
    // Start is called before the first frame update
    void Awake()
    {
        cover = GetComponentInChildren<CoverObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(cover != null) {
            cover = GetComponentInChildren<CoverObject>();
        }
    }
}
