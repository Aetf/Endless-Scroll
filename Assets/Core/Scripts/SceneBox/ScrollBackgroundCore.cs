using UnityEngine;
using System.Collections.Generic;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Character;

public class ScrollBackgroundCore : MonoBehaviour {

    public float scrollSpeed = 0.3f;
    public Transform target;
    public bool useMonoTarget = true;

    private float camHorzExtent;
    private float camVertExtent;

    private Renderer groundRenderer;

    private float scrollPosition;
    private float lastX;
    private Vector2 savedOffset;

    // Use this for initialization
    void Start() {
        if (useMonoTarget) {
            target = MonoTargetHub.Player;
        }
        // Setup references
        groundRenderer = transform.GetChild(0).gameObject.GetComponent<Renderer>();

        // Set and apply initial values
        scrollPosition = 0f;
        lastX = target.position.x;
        savedOffset = groundRenderer.sharedMaterial.mainTextureOffset;
        Scroll(0f);
    }

    // Update is called once per frame
    void Update() {
        TrackPlayer();
    }

    public void OnDestroy() {
        groundRenderer.sharedMaterial.mainTextureOffset = savedOffset;
    }

    void TrackPlayer() {
        float delta = target.position.x - lastX;
        lastX = target.position.x;
        Scroll(delta);
    }

    void Scroll(float delta) {
        scrollPosition += delta * scrollSpeed;

        float x = Mathf.Repeat(scrollPosition, 1);
        Vector2 offset = new Vector2(x, 0);
        groundRenderer.sharedMaterial.mainTextureOffset = offset;
    }
}
