using UnityEngine;
using System.Collections;
using System;

namespace UnlimitedCodeWorks.Events {

    /// <summary>
    /// Events that can be canceled
    /// </summary>
    public interface Cancelable {
        /// <summary>
        /// Whether or not cancel this event
        /// </summary>
        bool Cancel { get; set; }
    }

    namespace Character {
        /// <summary>
        /// Base class for all character related event
        /// </summary>
        public class CharacterEvent : GameEvent {
            /// <summary>
            /// Construct a CharacterEvent
            /// </summary>
            /// <param name="character"></param>
            public CharacterEvent(GameObject character) {
                Character = character;
            }

            /// <summary>
            /// Reference to the character GameObject which generated this event
            /// </summary>
            public GameObject Character { get; private set; }
        }

        /// <summary>
        /// Base class for all character moving event
        /// </summary>
        public class CharacterMoving : CharacterEvent, Cancelable {
            /// <summary>
            /// Construct a CharacterMoving event
            /// </summary>
            /// <param name="character"></param>
            /// <param name="destination"></param>
            public CharacterMoving(GameObject character, Vector3 destination)
                : base(character) {
                Destination = destination;
            }

            /// <summary>
            /// The destination the character is about to move to.
            /// </summary>
            public Vector3 Destination { get; private set; }

            #region Cancelable Interface
            public bool Cancel { get; set; }
            #endregion
        }

        /// <summary>
        /// Base class for all character moved event
        /// </summary>
        public class CharacterMoved : CharacterEvent {
            /// <summary>
            /// Construct a CharacterMoved event
            /// </summary>
            /// <param name="character"></param>
            public CharacterMoved(GameObject character) : base(character) {

            }
        }

        /// <summary>
        /// Raised before a player attempt to move
        /// </summary>
        public class PlayerMoving : CharacterMoving {
            /// <summary>
            /// Construct a PlayerMoving event
            /// </summary>
            /// <param name="player"></param>
            /// <param name="destination"></param>
            public PlayerMoving(GameObject player, Vector3 destination)
                : base(player, destination) {

            }
        }

        /// <summary>
        /// Raised after a player moved
        /// </summary>
        public class PlayerMoved : CharacterMoved {
            /// <summary>
            /// Construct a PlayerMoved event
            /// </summary>
            /// <param name="player"></param>
            public PlayerMoved(GameObject player) : base(player) { }
        }
    }
}
