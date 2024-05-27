using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Squad : MonoBehaviour
{
    public List<IAICharacterController> characters = new List<IAICharacterController>();
    public Vector3 lastKnownPosition { get; set; }

    void Start()
    {
        characters = GetComponentsInChildren<IAICharacterController>().ToList();
        foreach(var character in characters) {
            character.setSquad(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}