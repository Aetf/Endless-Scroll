using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
    public Vector2 offset;
	public float xMargin = 1f;		// Distance in the x axis the player can move before the camera follows.
	public float yMargin = 0f;		// Distance in the y axis the player can move before the camera follows.
    public float xOvershot = 0f;    // Distance in the x axis the camera moves over the player.
    public float yOvershot = 0f;    // Distance in the x axis the camera moves over the player.
	public float xSmooth = 8f;		// How smoothly the camera catches up with it's target movement in the x axis.
	public float ySmooth = 8f;		// How smoothly the camera catches up with it's target movement in the y axis.


	private Transform player;		// Reference to the player's transform.
    private Camera theCamera;
    private float horizExtent;
    private float vertExtent;
    private Bounds targetBounds;


	void Awake ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag("Player").transform;
        theCamera = GetComponent<Camera>();

        // Calculate camera extent
        vertExtent = theCamera.orthographicSize;
        horizExtent = theCamera.aspect * vertExtent;
	}

    public void SetMovingBounds(Bounds bounds) {
        Vector3 extents = bounds.extents;
        Vector3 center = bounds.center;
        extents.x -= horizExtent;
        extents.y -= vertExtent;
        extents.z = transform.position.z;
        center.z = transform.position.z;
        targetBounds.extents = extents;
        targetBounds.center = bounds.center;
    }

	bool CheckXMargin()
	{
		// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
		return Mathf.Abs(transform.position.x - offset.x - player.position.x) > xMargin;
	}


	bool CheckYMargin()
	{
		// Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
		return Mathf.Abs(transform.position.y - offset.y - player.position.y) > yMargin;
	}


	void FixedUpdate ()
	{
		TrackPlayer();
	}
	
	
	void TrackPlayer ()
	{
		// By default the target x and y coordinates of the camera are it's current x and y coordinates.
		float targetX = transform.position.x;
		float targetY = transform.position.y;

		// If the player has moved beyond the x margin...
		if(CheckXMargin())
			// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
			targetX = Mathf.Lerp(transform.position.x, player.position.x + offset.x + xOvershot, xSmooth * Time.deltaTime);

		// If the player has moved beyond the y margin...
		if(CheckYMargin())
			// ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
			targetY = Mathf.Lerp(transform.position.y, player.position.y + offset.y + yOvershot, ySmooth * Time.deltaTime);

        // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.

        Vector3 target = targetBounds.ClosestPoint(new Vector3(targetX, targetY, 0f));

		//targetX = Mathf.Clamp(targetX, movingBounds.min.x, movingBounds.max.x);
		//targetY = Mathf.Clamp(targetY, movingBounds.min.y, movingBounds.max.y);

		// Set the camera's position to the target position with the same z component.
		transform.position = new Vector3(target.x, target.y, transform.position.z);
	}
}
