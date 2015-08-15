using UnityEngine;
using System.Collections;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Chunks;

public class DestroyerCore : MonoBehaviour {

    private Collider2D theCollider;

    public static DestroyerCore Create(GameObject parent, Vector2 position) {
        GameObject destroyer = Instantiate(PrefabsHub.ChunkDestroyer,
            position.ToVector3(),
            Quaternion.identity) as GameObject;
        destroyer.transform.SetParent(parent.transform, false);
        return destroyer.GetComponent<DestroyerCore>();
    }

    public void SetDestroyerEnabled(bool value) {
        theCollider.enabled = value;
        gameObject.SetActive(value);
    }

    public void TriggerDestroy(GameObject obj) {
        RaiseDestroyingEvent(obj);
    }

    void RaiseDestroyingEvent(GameObject chunk) {
        EventHub.RaiseEvent(new ChunkDestroying(this, chunk));
    }

    GameObject FindChunk(GameObject obj) {
        while (!obj.CompareTag("ChunkWrapper") && obj.transform.parent != null) {
            Debug.Log("FindChunk at " + obj.name);
            obj = obj.transform.parent.gameObject;
        }
        return !obj.CompareTag("ChunkWrapper") ? obj : null;
    }

    void Awake() {
        theCollider = GetComponent<Collider2D>();
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
