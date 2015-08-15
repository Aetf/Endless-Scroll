using UnityEngine;
using System.Collections;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Chunks;

public class CheckerCore : MonoBehaviour {

    /// <summary>
    /// The extent of the checker
    /// </summary>
    public Vector2 extent = new Vector2(0.5f, 0.5f);

    public static CheckerCore Create(GameObject parent, Vector2 position) {
        GameObject checker = Instantiate(PrefabsHub.ChunkChecker,
            position.ToVector3(),
            Quaternion.identity) as GameObject;
        checker.transform.SetParent(parent.transform, false);
        return checker.GetComponent<CheckerCore>();
    }

    public void SetCheckerEnabled(bool value) {
        gameObject.SetActive(value);
    }

    private RaycastHit2D[] linecastRes = new RaycastHit2D[2];

    void DoLineCastAt(Vector2 start, Vector2 to) {
        int cnt = Physics2D.LinecastNonAlloc(start, to, linecastRes);

        if (cnt == 0) {
            RaiseChunkMissingAt(start);
        }
    }

    void RaiseChunkMissingAt(Vector2 pos) {
        Debug.Log("Checker at " + transform.position + " :ChunkMissing event at " + pos);
        EventHub.RaiseEvent(new ChunkMissing(this, pos));
    }

	// Update is called once per frame
	void Update () {
        Vector2 pos = transform.position;

        DoLineCastAt(pos.Translate(0f, extent.y), pos.Translate(0f, -extent.y));
	}

    void OnDrawGizmos() {
        Gizmos.DrawCube(transform.position.Translate(0, extent.y, 0), new Vector3(1, 0.2f, 1));
        Gizmos.DrawCube(transform.position.Translate(0, -extent.y, 0), new Vector3(1, 0.2f, 1));
    }
}
