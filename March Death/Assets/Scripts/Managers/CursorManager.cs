
using UnityEngine;

namespace Managers {
    public class CursorManager : Utils.SingletonMono<CursorManager>
    {
        public enum cursor { DEFAULT = -1, NO_BUILDING_IN = 0}

        protected CursorManager() { }

        //fake dynamic loading of textures
        private cursor _currentCursor = cursor.DEFAULT;
        private Texture2D[] cursors = new Texture2D[1];
        private Vector2 size = new Vector2(16, 16);
        private bool cursorChanged = false;

        void Start()
        {
            // loading textures for cursors
            cursors[(int)cursor.NO_BUILDING_IN] = (Texture2D)Resources.Load("RedCross");
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
                    GUI.DrawTexture(mouseRect, cursors[(int)_currentCursor]);
                    
                    break;
            }
            if (cursorChanged) cursorChanged = false;
        }


        /// <summary>
        /// Sets a new cursor
        /// </summary>
        /// <param name="newCursor"></param>
        public void setCursor(cursor newCursor)
        {
            if (newCursor != _currentCursor)
            {
                _currentCursor = newCursor;
                cursorChanged = true;
            }
        }


    }
}