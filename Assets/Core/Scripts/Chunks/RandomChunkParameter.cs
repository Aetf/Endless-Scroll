using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RandomChunkParameter : MonoBehaviour {

    public Vector2 chunkSize = new Vector2(16, float.PositiveInfinity);

    private Transform planes = null;

    void Awake() {
        planes = transform.FindChild("Planes");

        int seed = (int) Time.time;
        Debug.Log("Seed is " + seed);
        Randomize(seed);
    }

    public void Randomize(int seed) {
        Random.seed = seed;

        RandomizePlaneExistance();
        RandomizePlanePositions();
    }

    void RandomizePlanePositions() {
        foreach (Transform plane in planes) {
            float x = Random.Range(-0.5f, 0.5f) * chunkSize.x;
            plane.position = plane.position.ChangeX(x);
        }
    }

    void RandomizePlaneExistance() {
        float fillPercent = Random.Range(0, 100);
        foreach (Transform plane in planes) {
            bool show = Random.Range(0, 100) > fillPercent ? false : true;
            plane.gameObject.SetActive(show);
        }
    }
}
