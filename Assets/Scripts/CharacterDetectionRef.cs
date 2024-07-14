using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDetectionRef : MonoBehaviour
{
    public Character character;

    private void Awake() {
        character = GetComponentInParent<Character>();
    }
}
