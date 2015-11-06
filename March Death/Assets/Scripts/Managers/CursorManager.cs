
using UnityEngine;

namespace Managers {
    public partial class CursorManager : Utils.SingletonMono<CursorManager>
    {
        public enum cursor {
            DEFAULT = -1,
            NO_BUILDING_IN = 0, /* a red cross */
            MAIN = 1,           /* a simple arrow cursor */
            POINTER = 2,        /* a hand pointing */
            SWORD = 3           /* a sword */
        }

        protected CursorManager() { }
        private Player _player;
        private UserInput _inputs;
        private cursor _currentCursor = cursor.DEFAULT;
        private int _numCursors = 4;
        private Texture2D[] _cursors;
        private Vector2 size = new Vector2(32, 32);
        private bool cursorChanged = false;

        void Start()
        {
            // loading textures for cursors
            _cursors = new Texture2D[_numCursors];
            _cursors[(int)cursor.NO_BUILDING_IN] = (Texture2D)Resources.Load("cursors/red-cross");
            _cursors[(int)cursor.MAIN] = (Texture2D)Resources.Load("cursors/main");
            _cursors[(int)cursor.POINTER] = (Texture2D)Resources.Load("cursors/pointer");
            _cursors[(int)cursor.SWORD] = (Texture2D)Resources.Load("cursors/sword");
            Cursor.visible = false;
        }


        /// <summary>
        /// Updates the cursor
        /// </summary>
        void OnGUI()
        {
            switch (_currentCursor)
            {
                case cursor.DEFAULT:
                    if(cursorChanged) Cursor.visible = true;
                    break;
                default:
                    if (cursorChanged) Cursor.visible = false;

                    Rect mouseRect = new Rect(Event.current.mousePosition, size);
                    GUI.DrawTexture(mouseRect, _cursors[(int)_currentCursor]);
                    
                    break;
            }
            if (cursorChanged) cursorChanged = false;
        }


        /// <summary>
        /// Sets a new cursor
        /// </summary>
        /// <param name="newCursor"></param>
        public void _setCursor(cursor newCursor)
        {
            if (newCursor != _currentCursor)
            {
                _currentCursor = newCursor;
                cursorChanged = true;
            }
        }


        /// <summary>
        /// Injects the player to observe statuses
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            _player = player;
        }

        /// <summary>
        /// Injects the input to observe statuses
        /// </summary>
        /// <param name="inputs"></param>
        public void SetInputs(UserInput inputs)
        {
            _inputs = inputs;
        }

    }
}