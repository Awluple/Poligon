using System.Collections.Generic;
using UnityEngine;

public class HidingCollisionSphere : MonoBehaviour {
    Dictionary<GameObject, GameObject> covers = new Dictionary<GameObject, GameObject>();
    [Range(-1, 1)]
    [SerializeField] float sensitivity = 0.3f;
    [SerializeField] float minDistanceFromPlayer = 8f;


    public Dictionary<GameObject, GameObject> GetCovers() {
        return covers;
    }

    void OnCollisionEnter(Collision collision) {
        bool isCover = collision.GetContact(0).otherCollider.gameObject.TryGetComponent<Cover>(out Cover cover);
        if (isCover) covers.Add(cover.gameObject, cover.gameObject);
    }

    private void OnCollisionExit(Collision collision) {
        covers.Remove(collision.gameObject);
    }
}
