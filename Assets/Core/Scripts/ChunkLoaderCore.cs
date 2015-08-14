using UnityEngine;
using System;
using System.Collections.Generic;
using UnlimitedCodeWorks;
using UnlimitedCodeWorks.Events.Chunks;
using UnlimitedCodeWorks.Events.Character;

public class ChunkLoaderCore : MonoBehaviour {

    #region SceneBox check
    private GameObject sceneBox;
    void CheckForSceneBox() {
        if (transform.parent == null
            || transform.parent.gameObject.name != "SceneBox") {
            Debug.LogWarning("ChunkLoader must be parented to SceneBox to function correctly.");
            Debug.Break();
        }
        sceneBox = transform.parent.gameObject;

        // Reset position relative to SceneBox
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    #endregion

    #region Chunk repository
    private GameObject chunkRepository;

    void InitializeChunkRepository() {
        chunkRepository = GameObject.Find("ChunkRepository") as GameObject;
        if (chunkRepository) {
            // use already existing repository, reset its transform just in case...
            chunkRepository.transform.position = new Vector3(0, 0, 0);
            chunkRepository.transform.rotation = Quaternion.identity;
        } else {
            chunkRepository = new GameObject("ChunkRepository");
        }
    }
    #endregion

    #region Chunk size & count
    public Vector2 chunkSize = new Vector2(16f, float.PositiveInfinity);

    /// <summary>
    /// Minimum chunks to have
    /// </summary>
    public int minimumChunk = 2;

    /// <summary>
    /// In addition to `minimumChunk`, load `preloadChunk` chunks.
    /// </summary>
    public int preloadChunk = 0;

    private float markerY = -10f;

    void InitializeChunkSize() {
        // nothing to be done. we set chunk size manually.
    }
    #endregion

    #region Checkers
    private GameObject checkersParent = null;
    private CheckerCore leftChecker = null;
    private CheckerCore rightChecker = null;
    private List<CheckerCore> middleCheckers = new List<CheckerCore>();
    private bool checkerInitialized = false;

    float InitializeCheckers() {
        // Make sure this is the only chunk loader activated.
        EventHub.ClearListener<ChunkMissing>();

        // Listen to ChunkMissing event
        EventHub.AddListener<ChunkMissing>(e => {
            EnsureChunkAround(e.Position);
            if (e.Checker != leftChecker && e.Checker != rightChecker) {
                // middle checkers are disabled once used.
                Debug.Log("Disable checker at " + e.Checker.transform.position);
                e.Checker.SetCheckerEnabled(false);
            }
        });

        // Cleanup any old checkers;
        CleanupCheckers();
        // Make all checkers' parent object
        checkersParent = new GameObject("checkers");
        checkersParent.transform.parent = transform;

        // Calculate positions and create checkers
        int chunksToMaintain = minimumChunk + preloadChunk;
        // we need 2 additional checkers to ensure chunks are loaded at edge
        int totalCheckers = chunksToMaintain + 2;
        int halfMiddle = totalCheckers / 2 - 1;
        float chunkExtent = chunkSize.x / 2f;
        Func<float, float> createCheckers = (start) => {
            // left side
            float x = - start; // start from `-chunkExtent`, not `-=`
            for (int i = 0; i!= halfMiddle; i++) {
                middleCheckers.Add(CheckerCore.Create(checkersParent, new Vector2(x, markerY)));
                x -= chunkSize.x;
            }
            leftChecker = CheckerCore.Create(checkersParent, new Vector2(x, markerY));
            // right side
            x = start; // start from `chunkExtent`, not `+=`
            for (int i = 0; i!= halfMiddle; i++) {
                middleCheckers.Add(CheckerCore.Create(checkersParent, new Vector2(x, markerY)));
                x += chunkSize.x;
            }
            rightChecker = CheckerCore.Create(checkersParent, new Vector2(x, markerY));

            checkerInitialized = true;
            return x;
        };
        if (chunksToMaintain % 2 == 0) {
            return createCheckers(chunkExtent);
        } else {
            // a central one first
            middleCheckers.Add(CheckerCore.Create(checkersParent, new Vector2(0f, markerY)));
            // then others
            return createCheckers(chunkSize.x);
        }
    }

    void SetEnableMiddleCheckers(bool active) {
        middleCheckers.ForEach(checker => checker.SetCheckerEnabled(active));
    }

    void CleanupCheckers() {
        if (checkersParent != null)
            Destroy(checkersParent);
        middleCheckers.Clear();
    }
    #endregion

    #region Chunk load & unload
    void EnsureChunkAround(Vector2 position) {
        LoadChunkAtIndex(IndexForPosition(position));
    }

    int IndexForPosition(Vector2 position) {
        int idx = Mathf.FloorToInt(position.x / chunkSize.x);
        return idx;
    }

    void LoadChunkAtIndex(int idx) {
        // Get the chunk
        GameObject chunk = WrapChunk(Instantiate(PrefabsHub.ChunkContent));
        // Move to correct position
        float x = idx * chunkSize.x + chunkSize.x / 2;
        chunk.transform.position = chunk.transform.position.Translate(x, 0f, 0f);
        // Parent the chunk to repository
        chunk.transform.SetParent(chunkRepository.transform, true);
        chunk.name = "ChunkWrapper " + idx;
    }

    GameObject WrapChunk(GameObject chunk) {
        GameObject wrapper = Instantiate(PrefabsHub.ChunkWrapper);
        // Make sure the position are correct
        wrapper.transform.Reset();
        chunk.transform.Reset();
        // Adjust marker
        BoxCollider2D marker = wrapper.GetComponentInChildren<BoxCollider2D>();
        marker.size = new Vector2(chunkSize.x, 0.5f);
        marker.offset = new Vector2(0f, markerY);
        // Parent chunk content to wrapper
        chunk.transform.parent = wrapper.transform;

        return wrapper;
    }

    void DestroyChunk(GameObject chunk) {
        Destroy(chunk);
    }

    void UnloadChunkAtIndex(int idx) {
        Debug.LogWarning("UnloadChunkAtIndex: Not implemented yet.");
    }
    #endregion

    #region Destroyer
    private GameObject destroyersParent = null;
    private DestroyerCore leftDestroyer = null;
    private DestroyerCore rightDestroyer = null;
    private bool destroyerInitialized = false;

    void InitializedDestroyers(float minMargin) {
        // Make sure this is the only chunk unloader activated.
        EventHub.ClearListener<ChunkDestroying>();
        // Listen to ChunkDestroying event.
        EventHub.AddListener<ChunkDestroying>(e => DestroyChunk(e.Chunk));

        // Cleanup any old destroyers
        CleanupDestroyers();
        // Make destroyers' parent
        destroyersParent = new GameObject("destroyers");
        destroyersParent.transform.parent = transform;

        // Calculate positions and create destroyers
        // destroyers must be enough far from checkers to prevent infinite load/unload
        float margin = minMargin + chunkSize.x;
        leftDestroyer = DestroyerCore.Create(destroyersParent, new Vector2(-margin, markerY));
        rightDestroyer = DestroyerCore.Create(destroyersParent, new Vector2(margin, markerY));
        destroyerInitialized = true;
    }

    void CleanupDestroyers() {
        if (destroyersParent != null)
            Destroy(destroyersParent);
    }
    #endregion

    // Use this for initialization
    void Start () {
        CheckForSceneBox();
        InitializeChunkSize();
        InitializeChunkRepository();
        float farmostChecker = InitializeCheckers();
        InitializedDestroyers(farmostChecker);

        // React to player facing change event
        EventHub.AddListener<PlayerFacingChanged>(e => {
            if (checkerInitialized) {
                leftChecker.SetCheckerEnabled(!e.FacingRight);
                rightChecker.SetCheckerEnabled(e.FacingRight);
            }
            if (destroyerInitialized) {
                leftDestroyer.SetDestroyerEnabled(e.FacingRight);
                rightDestroyer.SetDestroyerEnabled(!e.FacingRight);
            }
        });
	}
}
