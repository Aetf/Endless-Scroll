using UnityEngine;
using System.Collections;

namespace UnlimitedCodeWorks {
    public static class MonoTargetHub {
        /// <summary>
        /// The gamer
        /// </summary>
        public static Transform Player { get { return Private.MonoTargetHub.Instance.Player; } }
    }

    namespace Private {
        class MonoTargetHub : MonoBehaviour {

            public Transform Player = null;

            #region Singleton
            private static MonoTargetHub instance = null;
            public static MonoTargetHub Instance {
                get {
                    if (!instance) {
                        instance = GameObject.FindObjectOfType(typeof(MonoTargetHub)) as MonoTargetHub;
                        // Automatically create the PrefabsHub if none was found upon start of the game.
                        if (!instance)
                            instance = (new GameObject("MonoTargetHub")).AddComponent<MonoTargetHub>();
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