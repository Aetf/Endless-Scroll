using UnityEngine;
using System.Collections;

namespace UnlimitedCodeWorks {

    public class FollowPlayer : MonoBehaviour {

        public bool enableXFollow = true;
        public bool enableYFollow = true;

        public Vector2 offset;
        public Vector2 margin = new Vector2(1, 1); // Distance the player can move before object follows.
        public Vector2 smooth = new Vector2(8, 8); // How smoothly the object catches up with it's target movement.

        private Transform player;       // Reference to the player's transform.

        void Awake() {
            // Setting up the reference.
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        bool CheckXMargin() {
            // Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
            return Mathf.Abs(transform.position.x - offset.x - player.position.x) > margin.x;
        }


        bool CheckYMargin() {
            // Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
            return Mathf.Abs(transform.position.y - offset.y - player.position.y) > margin.y;
        }


        void FixedUpdate() {
            TrackPlayer();
        }

        void TrackPlayer() {
            // By default the target x and y coordinates are it's current x and y coordinates.
            float targetX = transform.position.x;
            float targetY = transform.position.y;

            // If the player has moved beyond the x margin...
            if (enableXFollow && CheckXMargin())
                // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
                targetX = Mathf.Lerp(transform.position.x, player.position.x + offset.x, smooth.x * Time.deltaTime);

            // If the player has moved beyond the y margin...
            if (enableYFollow && CheckYMargin())
                // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
                targetY = Mathf.Lerp(transform.position.y, player.position.y + offset.y, smooth.y * Time.deltaTime);

            // Set the camera's position to the target position with the same z component.
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
    }
}
