using UnityEngine;
using System.Collections;

namespace UnlimitedCodeWorks {
    public static class PrefabsHub {
        /// <summary>
        /// Chunk checker used in CheckerCore
        /// </summary>
        public static GameObject ChunkChecker { get { return Private.PrefabsHub.Instance.ChunkChecker; } }

        /// <summary>
        /// Chunk destroyer used in DestroyerCore
        /// </summary>
        public static GameObject ChunkDestroyer { get { return Private.PrefabsHub.Instance.ChunkDestroyer; } }

        /// <summary>
        /// Chunk wrapper used in ChunkLoaderCore
        /// </summary>
        public static GameObject ChunkWrapper { get { return Private.PrefabsHub.Instance.ChunkWrapper; } }

        /// <summary>
        /// The chunk content
        /// </summary>
        public static GameObject ChunkContent { get { return Private.PrefabsHub.Instance.ChunkContent; } }
    }

    namespace Private {
        class PrefabsHub : MonoBehaviour {

            public GameObject ChunkChecker = null;
            public GameObject ChunkDestroyer = null;
            public GameObject ChunkWrapper = null;
            public GameObject ChunkContent = null;

            #region Singleton
            private static PrefabsHub instance = null;
            public static PrefabsHub Instance {
                get {
                    if (!instance) {
                        instance = GameObject.FindObjectOfType(typeof(PrefabsHub)) as PrefabsHub;
                        // Automatically create the PrefabsHub if none was found upon start of the game.
                        if (!instance)
                            instance = (new GameObject("PrefabsHub")).AddComponent<PrefabsHub>();
                    }
                    return instance;
                }
            }

            // Should be called in MonoBehaviour.Awake
            private void EnforceSingleton() {
                if (Instance != this) {
                    Destroy(gameObject);
                }
                DontDestroyOnLoad(gameObject);
            }
            #endregion

            void Awake() {
                EnforceSingleton();
            }
        }
    }
}
