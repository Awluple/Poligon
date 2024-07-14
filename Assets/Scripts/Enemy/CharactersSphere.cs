using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersSphere : MonoBehaviour
{
    Dictionary<GameObject, Character> characters = new Dictionary<GameObject, Character>();
    private Character parent;
    public void Awake() {
        parent = GetComponentInParent<Character>();
    }
    public Dictionary<GameObject, Character> GetCharacters() {
        return characters;
    }

    void OnCollisionEnter(Collision collision) {
        bool isCharacter = collision.GetContact(0).otherCollider.gameObject.TryGetComponent<CharacterDetectionRef>(out CharacterDetectionRef characterRef);
        if (isCharacter && characterRef.character != parent) {
            characters.Add(characterRef.gameObject, characterRef.character);
            Debug.Log("Character Added" + " " + characterRef.character.name);
            Debug.Log(characters.Count);
        }
    }

    private void OnCollisionExit(Collision collision) {
        characters.Remove(collision.gameObject);
        Debug.Log("Character Removed!");
        Debug.Log(characters.Count);
    }
}
