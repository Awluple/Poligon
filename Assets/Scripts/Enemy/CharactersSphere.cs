using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poligon.Enums;

public class CharactersSphere : MonoBehaviour
{
    Dictionary<GameObject, Character> enemyCharacters = new Dictionary<GameObject, Character>();
    Dictionary<GameObject, Character> friendlyCharacters = new Dictionary<GameObject, Character>();

    private Character parent;
    public void Awake() {
        parent = GetComponentInParent<Character>();
    }
    public Dictionary<GameObject, Character> GetEnemyCharacters() {
        return enemyCharacters;
    }
    public Dictionary<GameObject, Character> GetFriendlyCharacters() {
        return enemyCharacters;
    }

    void OnCollisionEnter(Collision collision) {
        bool isCharacter = collision.GetContact(0).otherCollider.gameObject.TryGetComponent<CharacterDetectionRef>(out CharacterDetectionRef characterRef);
        if (isCharacter && characterRef.character != parent) {
            if(characterRef.character.team == Team.Friendly && parent.team == Team.Enemy) {
                enemyCharacters.Add(characterRef.gameObject, characterRef.character);
            } else {
                friendlyCharacters.Add(characterRef.gameObject, characterRef.character);
            }

        }
    }

    private void OnCollisionExit(Collision collision) {
        if(enemyCharacters.ContainsKey(collision.gameObject)) {
            enemyCharacters.Remove(collision.gameObject);
        } else {
            friendlyCharacters.Remove(collision.gameObject);
        }

    }
}
