using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : Gun
{
    // Start is called before the first frame update
    void Start()
    {
        positionOnBody = FindFirstObjectByType<AssultRiflePosition>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
