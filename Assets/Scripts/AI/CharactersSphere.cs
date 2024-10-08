using Poligon.Enums;
using Poligon.EvetArgs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharactersSphere : MonoBehaviour {
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
            if (characterRef.character.team == Team.Friendly && parent.team == Team.Enemy ||
                characterRef.character.team == Team.Enemy && parent.team == Team.Friendly) {
                enemyCharacters.Add(characterRef.gameObject, characterRef.character);
            } else {
                friendlyCharacters.Add(characterRef.gameObject, characterRef.character);
            }
            characterRef.character.OnDeath += RemoveCharacter;

        }
    }

    private void OnCollisionExit(Collision collision) {
        if (enemyCharacters.ContainsKey(collision.gameObject)) {
            enemyCharacters.Remove(collision.gameObject);
        } else {
            friendlyCharacters.Remove(collision.gameObject);
        }
    }

    private void RemoveCharacter(object sender, CharacterEventArgs e) {
        if (e.character.team == Team.Friendly) {
            friendlyCharacters.Remove(e.character.gameObject);
        } else {
            enemyCharacters.Remove(e.character.gameObject);
        }
    }
}
