using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HidingCollisionSphere : MonoBehaviour
{
    Dictionary<GameObject, ContactPoint> covers = new Dictionary<GameObject, ContactPoint>();
    Player player;
    [Range(-1,1)]
    [SerializeField] float sensitivity = 0.3f;
    [SerializeField] float minDistanceFromPlayer = 8f;

    private void Awake() {
        player = FindObjectOfType<Player>();
    }

    public Dictionary<GameObject, ContactPoint> GetCovers() {
        return covers;
    }

    void OnCollisionEnter(Collision collision) {
        covers.Add(collision.GetContact(0).otherCollider.gameObject, collision.GetContact(0));
    }

    private void OnCollisionExit(Collision collision) {
        covers.Remove(collision.gameObject);
    }
}
