using UnityEngine;

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

    namespace Chunks {
        /// <summary>
        /// Raised when a chunk missing is detected by a checker.
        /// </summary>
        public class ChunkMissing : GameEvent {
            /// <summary>
            /// The exact position the missing detected.
            /// </summary>
            public Vector2 Position { get; private set; }

            /// <summary>
            /// The checker that detected the missing.
            /// </summary>
            public CheckerCore Checker { get; private set; }

            public ChunkMissing(CheckerCore checker, Vector2 position) {
                Checker = checker;
                Position = position;
            }
        }

        /// <summary>
        /// Raised when a destroyer collides with a chunk
        /// </summary>
        public class ChunkDestroying : GameEvent {
            /// <summary>
            /// The chunk the collision occurred.
            /// </summary>
            public GameObject Chunk { get; private set; }

            /// <summary>
            /// The destroyer raising the event
            /// </summary>
            public DestroyerCore Destroyer { get; private set; }

            public ChunkDestroying(DestroyerCore destroyer, GameObject chunk) {
                Destroyer = destroyer;
                Chunk = chunk;
            }
        }

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

        /// <summary>
        /// Raised after a player facing direction changed
        /// </summary>
        public class PlayerFacingChanged : CharacterEvent {
            /// <summary>
            /// Player's new facing direction
            /// </summary>
            public bool FacingRight { get; private set; }

            public PlayerFacingChanged(GameObject player, bool isFacingRight)
                : base(player) {
                FacingRight = isFacingRight;
            }
        }
    }
}
