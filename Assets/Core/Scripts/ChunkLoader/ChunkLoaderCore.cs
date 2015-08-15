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
    private CheckerCore edgeChecker = null;
    private List<CheckerCore> middleCheckers = new List<CheckerCore>();
    private Vector2 leftCheckerOffset;
    private Vector2 rightCheckerOffset;
    private bool checkerFacingRight;
    private bool checkerInitialized = false;

    float InitializeCheckers() {
        // Make sure this is the only chunk loader activated.
        EventHub.ClearListener<ChunkMissing>();

        // Listen to ChunkMissing event
        EventHub.AddListener<ChunkMissing>(e => {
            EnsureChunkAround(e.Position);
            if (e.Checker != edgeChecker) {
                // middle checkers are disabled once used.
                Debug.Log("Disable checker at " + e.Checker.transform.position);
                e.Checker.SetCheckerEnabled(false);
            }
        });

        // Cleanup any old checkers;
        CleanupCheckers();
        // Make all checkers' parent object
        checkersParent = new GameObject("Checkers");
        checkersParent.transform.Reset();
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
            leftCheckerOffset = new Vector2(x, markerY);
            // right side
            x = start; // start from `chunkExtent`, not `+=`
            for (int i = 0; i!= halfMiddle; i++) {
                middleCheckers.Add(CheckerCore.Create(checkersParent, new Vector2(x, markerY)));
                x += chunkSize.x;
            }
            rightCheckerOffset = new Vector2(x, markerY);

            // Force a checker facing change to initialize edge checker
            checkerFacingRight = false;
            SetCheckerFacingRight(true);

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
        edgeChecker = null;
    }

    void SetCheckerFacingRight(bool facingRight) {
        if (checkerFacingRight == facingRight)
            return;
        checkerFacingRight = facingRight;
        Vector2 pos = checkerFacingRight ? rightCheckerOffset : leftCheckerOffset;
        if (edgeChecker == null) {
            edgeChecker = CheckerCore.Create(checkersParent, pos);
        } else {
            edgeChecker.transform.localPosition = pos.ToVector3();
        }
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
    private DestroyerCore edgeDestroyer = null;
    private Vector2 leftDestroyerOffset;
    private Vector2 rightDestroyerOffset;
    private bool destroyerFacingRight;
    private bool destroyerInitialized = false;

    void InitializedDestroyers(float minMargin) {
        // Make sure this is the only chunk unloader activated.
        EventHub.ClearListener<ChunkDestroying>();
        // Listen to ChunkDestroying event.
        EventHub.AddListener<ChunkDestroying>(e => DestroyChunk(e.Chunk));

        // Cleanup any old destroyers
        CleanupDestroyers();
        // Make destroyers' parent
        destroyersParent = new GameObject("Destroyers");
        destroyersParent.transform.Reset();
        destroyersParent.transform.parent = transform;

        // Calculate positions and create destroyers
        // destroyers must be enough far from checkers to prevent infinite load/unload
        float margin = Mathf.Abs(leftCheckerOffset.x) + chunkSize.x;
        leftDestroyerOffset = new Vector2(-margin, markerY);
        rightDestroyerOffset = new Vector2(margin, markerY);

        // Force a facing change to initialize the edge destroyer
        destroyerFacingRight = false;
        SetDestroyerFacingRight(true);

        destroyerInitialized = true;
    }

    void CleanupDestroyers() {
        if (destroyersParent != null)
            Destroy(destroyersParent);
    }

    void SetDestroyerFacingRight(bool facingRight) {
        if (destroyerFacingRight == facingRight)
            return;
        destroyerFacingRight = facingRight;
        Vector2 pos = destroyerFacingRight ? leftDestroyerOffset : rightDestroyerOffset;
        if (edgeDestroyer == null) {
            edgeDestroyer = DestroyerCore.Create(destroyersParent, pos);
        } else {
            edgeDestroyer.transform.localPosition = pos;
        }
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
                SetCheckerFacingRight(e.FacingRight);
            }
            if (destroyerInitialized) {
                SetDestroyerFacingRight(e.FacingRight);
            }
        });
	}
}
