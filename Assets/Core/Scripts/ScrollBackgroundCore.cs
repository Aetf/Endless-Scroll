using UnityEngine;
using System.Collections.Generic;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Character;

public class ScrollBackgroundCore : MonoBehaviour {

    public float scrollSpeed = 0.3f;

    private float camHorzExtent;
    private float camVertExtent;

    private Renderer groundRenderer;
    private Transform playerTrans;

    private float scrollPosition;
    private float lastX;

    // Use this for initialization
    void Start () {
        // Setup references
        groundRenderer = transform.GetChild(0).gameObject.GetComponent<Renderer>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        // Set and apply initial values
        scrollPosition = 0f;
        lastX = playerTrans.position.x;
        Scroll(0f);
	}
	
	// Update is called once per frame
	void Update () {
        TrackPlayer();
	}

    void TrackPlayer() {
        float delta = playerTrans.position.x - lastX;
        lastX = playerTrans.position.x;
        Scroll(delta);
    }

    void Scroll(float delta) {
        scrollPosition += delta * scrollSpeed;

        float x = Mathf.Repeat(scrollPosition, 1);
        Vector2 offset = new Vector2(x, 0);
        groundRenderer.sharedMaterial.mainTextureOffset = offset;
    }
}
