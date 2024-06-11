using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun {

    // Start is called before the first frame update
    void Start()
    {
        positionOnBody = FindFirstObjectByType<PistolPosition>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
