using UnityEngine;
using System.Collections;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Character;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EventHub.AddListener<PlayerMoving>(OnPlayerMoving);
        EventHub.AddListener<PlayerMoved>(OnPlayerMoved);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnPlayerMoved(PlayerMoved evt) {
        Vector3 delta = evt.Character.transform.position - transform.position;
        delta.z = 0;

        transform.Translate(delta);
    }

    void OnPlayerMoving(PlayerMoving evt) {

    }
}
