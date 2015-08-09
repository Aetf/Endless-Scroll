using UnityEngine;
using System.Collections;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Character;

public class PlayerController : MonoBehaviour {

    private Rigidbody2D rb2d;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Horizontal")) {
            float deltaX = Input.GetAxis("Horizontal") * 0.05f;
            Move(deltaX);
        }
	}

    bool ShouldMove(Vector3 target) {
        PlayerMoving evt = new PlayerMoving(gameObject, target);
        EventHub.RaiseEvent(evt);
        return !evt.Cancel;
    }

    void Move(float delta) {
        Vector3 target = transform.position + new Vector3(delta, 0, 0);
        if (ShouldMove(target)) {
            transform.Translate(delta, 0, 0);
            EventHub.RaiseEvent(new PlayerMoved(gameObject));
        }
    }
}
