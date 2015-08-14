/*
 * Advanced C# Event Hub for Unity by Aetf. V1.1
 * 
 * Based on Ilya Suzdalnitski's "Advanced C# Messenger" (http://wiki.unity3d.com/index.php?title=Advanced_CSharp_Messenger)
 * and Ben D'Angelo's "C# Type-safe Event Manager" (http://www.bendangelo.me/unity3d/2014/12/24/unity3d-event-manager.html).
 * 
 * Features:
 *  + Automatically self-creation singleton, no need to manually add to scene.
 *  + Scoped type-safe events, only listen to interested events in some scope.
 *  + Event queue preventing the game from advancing forward too quickly (event trigger chains).
 *  + One shot event.
 *  + Automatic cleanup before level loading, preventing MissingReferenceException because of referencing to destroyed event handlers.
 *  + Extensive error detection, preventing silent bugs.
 *  + Ability to hook to ListenerAdding, ListenerRemoving, ListenerRemoved
 *  + Option to enable/disable log.
 * 
 * Usage examples:
 *  1. EventHub.AddListener<PropCollectedEvent>(OnPropCollected);
 *     EventHub.RaiseEvent(new PropCollectedEvent(prop));
 *  2. EventHub.AddListenerOnce<AnimFinished>(OnAnimFinished);
 *     EventHub.RaiseEvent(new AnimFinished());
 *  3. class BaseEvent : GameEvent { }
 *     class DerivedEvent : BaseEvent { }
 *
 *     EventHub.AddListener<BaseEvent>(OnBase);
 *     EventHub.RaiseEvent(new DerivedEvent());
 * 
 * Note:
 * EvenTable is automatically cleaned up upon loading of a new level.
 * Don't forget that the messages that should survive the cleanup, should be marked with EventHub.MarkAsPermanent(string)
 * 
 */

// Enable/disable hooking and logging
//#define HOOKING_ENABLED
//#define LOG_ENABLED

// Enable/disable checks, requiring hooking enabled
//#define CHECK_REMOVING_UNKNOWN
//#define CHECK_QUEUE_NON_LISTENER_EVENT
//#define CHECK_REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnlimitedCodeWorks {
    /// <summary>
    /// Base class for all event. New event must derive from this class
    /// </summary>
    public class GameEvent {

        /// <summary>
        /// Set this to true to prevent propagation in event classes' hierarchy.  
        /// </summary>
        public bool StopPropagation { get; set; }

        protected GameEvent() {
            StopPropagation = false;
        }
    }

    public static class EventHub {

        #region Public classes
        public delegate void EventDelegate<T>(T e) where T : GameEvent;
        public delegate void EventDelegate(GameEvent evt);

        public class RaisingEventException : Exception {
            public RaisingEventException(string msg)
                : base(msg) {
            }
        }

        public class ListenerException : Exception {
            public ListenerException(string msg)
                : base(msg) {
            }
        }
        #endregion

        /// <summary>
        /// Add listener for particular event type T, with optional name.
        /// And returns a delegate handle to the listener for later removal.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">Listener to add</param>
        /// <returns>Newly created delegate. Must pass this value to RemoveListener rather than the original delegate.</returns>
        public static EventDelegate AddListener<T>(EventDelegate<T> del) where T : GameEvent {
            return Private.EventHub.Instance.AddListener<T>(del);
        }

        /// <summary>
        /// Add one shot listener for particular event type T, with optional name.
        /// And returns a delegate handle to the listener for later removal.
        /// The listener will be removed automatically after being called once.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">Listener to add</param>
        /// <returns>Newly created delegate. Must pass this value to RemoveListener rather than the original delegate.</returns>
        public static EventHub.EventDelegate AddListenerOnce<T>(EventHub.EventDelegate<T> del) where T : GameEvent {
            return Private.EventHub.Instance.AddListenerOnce<T>(del);
        }

        /// <summary>
        /// Clear all listeners registered on event T
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        public static void ClearListener<T>() {
            Private.EventHub.Instance.ClearListener<T>();
        }

        /// <summary>
        /// Remove given listener registered on event T
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">listener to remove, this must be a return value from either AddListener or AddListenerOnce</param>
        public static void RemoveListener<T>(EventHub.EventDelegate del) {
            Private.EventHub.Instance.RemoveListener<T>(del);
        }

        /// <summary>
        /// Raise the event immediately.
        /// </summary>
        /// <param name="evt">Event object</param>
        public static void RaiseEvent<T>(T evt) where T : GameEvent {
            Private.EventHub.Instance.RaiseEvent<T>(evt);
        }

        /// <summary>
        /// Inserts the event into the current queue for processing in later frames.
        /// </summary>
        /// <param name="evt">Event object</param>
        /// <returns>true if successfully enqueued</returns>
        public static bool QueueEvent<T>(T evt) where T : GameEvent {
            return Private.EventHub.Instance.QueueEvent<T>(evt);
        }

        /// <summary>
        /// Marks a certain event as permanent.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        public static void MarkAsPermanent<T>() {
            Private.EventHub.Instance.MarkAsPermanent<T>();
        }

        /// <summary>
        /// Are there any listeners on event T
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <returns>true if at least one listener is found</returns>
        public static bool HasListener<T>() {
            return Private.EventHub.Instance.HasListener(typeof(T));
        }

        /// <summary>
        /// Cleanup, discard all contents in event table, except for those marked as permanent.
        /// </summary>
        public static void Cleanup() {
            Private.EventHub.Instance.Cleanup();
        }

        /// <summary>
        /// Debug use. Print the whole event table.
        /// </summary>
        public static void PrintEventTable() {
            Private.EventHub.Instance.PrintEventTable();
        }
    }

    namespace Private {
        class EventHub : MonoBehaviour {

            #region Public variables
            /// <summary>
            /// Limit event processing time per frame or not.
            /// </summary>
            public bool LimitQueueProcessing = false;

            /// <summary>
            /// Maximum time allowed to process events per frame, in seconds.
            /// </summary>
            public float QueueProcessTime = 0.0f;
            #endregion

            #region Public util methods
            public void MarkAsPermanent<T>() {
                System.Type evtType = typeof(T);
                InternalLog(string.Format("MarkAsPermanent Event: {0}", evtType.Name));

                if (permanentEventTable.ContainsKey(evtType)) {
                    return;
                } else if (eventTable.ContainsKey(evtType)) {
                    permanentEventTable[evtType] = eventTable[evtType];
                    eventTable.Remove(evtType);
                } else {
                    permanentEventTable[evtType] = null;
                }
            }

            public void Cleanup() {
                InternalLog("Cleanup. Make sure that none of necessary listeners are removed.");

                eventTable.Clear();
            }

            public bool HasListener(System.Type evtType) {
                UnlimitedCodeWorks.EventHub.EventDelegate del;
                return (permanentEventTable.TryGetValue(evtType, out del) && del != null)
                        || eventTable.ContainsKey(evtType);
            }

            public void PrintEventTable() {
                InternalLog("=== Messaging Center EventTable (Permanent) ===");
                foreach (KeyValuePair<Type, UnlimitedCodeWorks.EventHub.EventDelegate> pair in permanentEventTable) {
                    InternalLog(pair.Key + "\t\t" + pair.Value);
                }

                InternalLog("=== Messaging Center EventTable ===");
                foreach (KeyValuePair<Type, UnlimitedCodeWorks.EventHub.EventDelegate> pair in eventTable) {
                    InternalLog(pair.Key + "\t\t" + pair.Value);
                }

                InternalLog("\n");
            }
            #endregion

            #region Add listener
            public UnlimitedCodeWorks.EventHub.EventDelegate AddListener<T>(UnlimitedCodeWorks.EventHub.EventDelegate<T> del) where T : GameEvent {
                return AddDelegate(del);
            }

            public UnlimitedCodeWorks.EventHub.EventDelegate AddListenerOnce<T>(UnlimitedCodeWorks.EventHub.EventDelegate<T> del) where T : GameEvent {
                return AddDelegate(del, true);
            }

            private UnlimitedCodeWorks.EventHub.EventDelegate AddDelegate<T>(UnlimitedCodeWorks.EventHub.EventDelegate<T> del, bool oneShot = false)
                where T : GameEvent {

                System.Type evtType = typeof(T);

#if HOOKING_ENABLED
        // Call hooks
        OnListenerAdding(evtType);
#endif

                // Create new delegate deleting itself after calling del.
                // 
                // NOTE: C# lambda captures references of local variables,
                // so even though `newDel` is null when the lambda evaluated,
                // the lambda works correctly with `newDel` assigned to itself later.
                //
                // Source: MSDN Lambda Expressions (C# Programming Guide) - Variable Scope in Lambda Expressions
                // URL: https://msdn.microsoft.com/en-us/library/bb397687.aspx
                UnlimitedCodeWorks.EventHub.EventDelegate newDel = null;
                if (oneShot) {
                    newDel = e => {
                        del((T) e);
                        RemoveListener<T>(newDel);
                    };
                } else {
                    newDel = e => del((T) e);
                }

                UnlimitedCodeWorks.EventHub.EventDelegate tempDel;
                // First check permanent event table, then the normal one.
                if (permanentEventTable.TryGetValue(evtType, out tempDel)) {
                    permanentEventTable[evtType] = tempDel += newDel;
                } else if (eventTable.TryGetValue(evtType, out tempDel)) {
                    eventTable[evtType] = tempDel += newDel;
                } else {
                    eventTable[evtType] = newDel;
                }

                return newDel;
            }
            #endregion

            #region Remove listener
            public void ClearListener<T>() {
                System.Type evtType = typeof(T);

#if HOOKING_ENABLED
        // Call hooks
        OnListenerRemoving(evtType);
#endif

                if (permanentEventTable.ContainsKey(evtType)) {
                    permanentEventTable[evtType] = null;

#if HOOKING_ENABLED
            // Call hooks
            OnListenerRemoved(evtType);
#endif
                } else if (eventTable.ContainsKey(evtType)) {
                    eventTable[evtType] = null;

#if HOOKING_ENABLED
            // Call hooks
            OnListenerRemoved(evtType);
#endif
                }
            }

            public void RemoveListener<T>(UnlimitedCodeWorks.EventHub.EventDelegate del) {
                System.Type evtType = typeof(T);

#if HOOKING_ENABLED
        // Call hooks
        OnListenerRemoving(evtType);
#endif

                UnlimitedCodeWorks.EventHub.EventDelegate tempDel;
                if (permanentEventTable.TryGetValue(evtType, out tempDel)) {
                    tempDel -= del;
                    permanentEventTable[evtType] = tempDel;

#if HOOKING_ENABLED
            // Call hooks
            OnListenerRemoved(evtType);
#endif
                } else if (eventTable.TryGetValue(evtType, out tempDel)) {
                    tempDel -= del;
                    if (tempDel == null) {
                        eventTable.Remove(evtType);
                    } else {
                        eventTable[evtType] = tempDel;
                    }

#if HOOKING_ENABLED
            // Call hooks
            OnListenerRemoved(evtType);
#endif
                }
            }
            #endregion

            #region Raise & queue event
            public void RaiseEvent<T>(T evt) where T : GameEvent {
                System.Type evtType = typeof(T);

                // Invoke listeners, from the most derived type to the one
                // directly inheriting `GameEvent`
                System.Type curEvtType = evtType;
                while (curEvtType != typeof(System.Object)) {
#if HOOKING_ENABLED
            // Call hooks
            OnRaising(curEvtType);
#endif

                    UnlimitedCodeWorks.EventHub.EventDelegate del;
                    if (permanentEventTable.TryGetValue(curEvtType, out del)) {
                        del(evt);
                    } else if (eventTable.TryGetValue(curEvtType, out del)) {
                        del(evt);
                    }

                    // stop propagation to base class if user requests that.
                    if (evt.StopPropagation)
                        break;

                    curEvtType = curEvtType.BaseType;
                }
            }

            public bool QueueEvent<T>(T evt)
                where T : GameEvent {

#if HOOKING_ENABLED
        // Call hooks
        OnEnqueuing(typeof(T));
#endif

                eventQueue.Enqueue(evt);
                return true;
            }

            // Should be called in MonoBehaviour.Update
            private void ProcessQueue() {
                //Every update cycle the queue is processed, if the queue processing is limited,
                //a maximum processing time per update can be set after which the events will have
                //to be processed next update loop.
                float timer = 0.0f;
                while (eventQueue.Count > 0) {
                    if (LimitQueueProcessing) {
                        if (timer > QueueProcessTime)
                            return;
                    }

                    GameEvent evt = eventQueue.Dequeue();
                    RaiseEvent(evt);

                    if (LimitQueueProcessing)
                        timer += Time.deltaTime;
                }
            }
            #endregion

            #region Internal variables
            // Main event table
            private Dictionary<System.Type, UnlimitedCodeWorks.EventHub.EventDelegate> eventTable = new Dictionary<Type, UnlimitedCodeWorks.EventHub.EventDelegate>();
            // Event handlers that should never be removed, regardless of calling Cleanup
            private Dictionary<System.Type, UnlimitedCodeWorks.EventHub.EventDelegate> permanentEventTable = new Dictionary<Type, UnlimitedCodeWorks.EventHub.EventDelegate>();

            // Queue for non-immediately processed events.
            private Queue<GameEvent> eventQueue = new Queue<GameEvent>();
            #endregion

            #region Hooking points
            protected void OnListenerAdding(System.Type evtType) {
                InternalLog(string.Format("Adding event {0}", evtType.Name));
            }

            protected void OnListenerRemoving(System.Type evtType) {
                InternalLog(string.Format("Removing event {0}", evtType.Name));
#if CHECK_REMOVING_UNKNOWN
        if (!HasListener(evtType)) {
            throw new EventHub.ListenerException(string.Format("Attempting to remove listener on event {0} which has no listeners.", evtType.Name));
        }
#endif
            }

            protected void OnListenerRemoved(System.Type evtType) {
            }

            protected void OnRaising(System.Type evtType) {
                InternalLog(string.Format("Raising event {0}", evtType.Name));
#if CHECK_REQUIRE_LISTENER
        if (!HasListener(evtType)) {
            throw new EventHub.RaisingEventException(string.Format("Raising event {0} but no listeners found."
                + " Try marking the listener with EventHub.MarkAsPermanent.", evtType.Name));
        }
#endif
            }

            protected void OnEnqueuing(System.Type evtType) {
                InternalLog(string.Format("Enqueuing event {0}", evtType.Name));
#if CHECK_QUEUE_NON_LISTENER_EVENT
        if (!HasListener(evtType)) {
            throw new EventHub.RaisingEventException("EventManager: QueueEvent failed due to no listeners for event " + evtType.Name);
        }
#endif
            }
            #endregion

            #region Logging
            private void InternalLog(string msg) {
#if LOG_ENABLED
        Debug.Log(string.Format("EventHub\t{0}\t\t\t{1}", System.DateTime.Now.ToString("hh:mm:ss.fff"), msg));
#endif
            }
            #endregion

            #region Singleton
            private static EventHub instance = null;
            public static EventHub Instance {
                get {
                    if (!instance) {
                        instance = GameObject.FindObjectOfType(typeof(EventHub)) as EventHub;
                        // Automatically create the EventHub if none was found upon start of the game.
                        if (!instance)
                            instance = (new GameObject("EventHub")).AddComponent<EventHub>();
                    }
                    return instance;
                }
            }

            // Should be called in MonoBehaviour.Awake
            private void EnforceSingleton() {
                if (Instance != this) {
                    InternalLog("Duplicate EventHub, destroying the new one.");
                    Destroy(gameObject);
                }
                DontDestroyOnLoad(gameObject);
            }
            #endregion

            #region Unity events handler
            void Awake() {
                EnforceSingleton();
            }

            // Update is called once per frame
            void Update() {
                ProcessQueue();
            }

            //Clean up eventTable every time a new level loads.
            public void OnLevelWasLoaded(int unused) {
                Cleanup();
            }

            public void OnApplicationQuit() {
                Cleanup();
                permanentEventTable.Clear();
                eventQueue.Clear();
                instance = null;
            }
            #endregion
        }
    }
}
