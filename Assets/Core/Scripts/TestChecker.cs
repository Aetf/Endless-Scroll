using UnityEngine;
using System.Collections;

public class TestChecker : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("TriggerEnter2D");
    }

    void OnTriggerStay2D(Collider2D other) {
        Debug.Log("TriggerStay2D");
    }

    public void OnTriggerExit2D(Collider2D collision) {
        Debug.Log("TriggerExit2D");
    }

}
