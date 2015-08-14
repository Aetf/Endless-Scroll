using UnityEngine;
using System.Collections;

public class MarkerLineCore : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("TriggerEnter2D received on marker " + transform.position);
        DestroyerCore destroyer = other.gameObject.GetComponent<DestroyerCore>();
        if (destroyer != null) {
            destroyer.TriggerDestroy(transform.parent.gameObject);
        }
    }
}
