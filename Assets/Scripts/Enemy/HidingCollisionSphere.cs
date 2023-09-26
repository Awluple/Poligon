using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HidingCollisionSphere : MonoBehaviour
{
    Dictionary<GameObject, GameObject> covers = new Dictionary<GameObject, GameObject>();
    Player player;
    [Range(-1,1)]
    [SerializeField] float sensitivity = 0.3f;
    [SerializeField] float minDistanceFromPlayer = 8f;

    private void Awake() {
        player = FindObjectOfType<Player>();
    }
    private void Start() {
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius, LayerMask.GetMask("Cover"));
        //foreach (Collider hitCollider in hitColliders) {
        //    covers.Add(hitCollider.gameObject, hitCollider.gameObject);
        //}
    }

    public Dictionary<GameObject, GameObject> GetCovers() {
        return covers;
    }

    void OnCollisionEnter(Collision collision) {
        covers.Add(collision.GetContact(0).otherCollider.gameObject, collision.GetContact(0).otherCollider.gameObject);
    }

    private void OnCollisionExit(Collision collision) {
        covers.Remove(collision.gameObject);
    }
}
