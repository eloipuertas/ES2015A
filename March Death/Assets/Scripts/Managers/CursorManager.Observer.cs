using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    public partial class CursorManager
    {

        void Update()
        {

            Observe();

        }

        private void Observe() {
            switch (_player.currently)
            {
                case Player.status.IDLE:
                    CheckIdle();
                    break;
                case Player.status.SELECTED_UNITS:
                    CheckSelected();
                    break;
                case Player.status.PLACING_BUILDING:
                    CheckPlacing();
                    break;
            }
                
        }

        /// <summary>
        /// Performs decissions when player is in idle state
        /// </summary>
        private void CheckIdle() {
            /// Mainly, if the player is idle, we need to check if it's dragging or not
            /// if it's dragging we'll show the pointer cursor, because is nice and I like it
            /// if it's not dragging we'll check if is over an ally or an enemy, if it's an ally we will show the pointer 
            /// and if it's an enemy we we'll show the main cursor

            UserInput.action action;

            action = _inputs.GetCurrentAction();

            switch (action)
            {
                case UserInput.action.DRAG:
                    // if its dragging we put the pointer cursor
                    _currentCursor = cursor.POINTER;
                    break;
                default :
                    if(ItsAlly())
                        _currentCursor = cursor.POINTER;
                    else
                       _currentCursor = cursor.MAIN;
                    break;
            }
           
        }


        /// <summary>
        /// Performs decissions when player is in SELECTED state
        /// </summary>
        private void CheckSelected()
        {
            /// Mainly here we check if the selection is not a building and then
            /// if the pointed entity is an enemy or not, if it is we show a sword if not, we show the pointer cursor

            if (_player.selection.IsBuilding())
                _currentCursor = cursor.POINTER;
            else if(ItsEnemy())
                _currentCursor = cursor.SWORD;
            else
                _currentCursor = cursor.POINTER;
            
                

        }


        /// <summary>
        /// Performs decission when the player is placing a building
        /// </summary>
        private void CheckPlacing()
        {
            ///Mainly we check if the current position of the building es valid or not
            switch (_player.buildings.currentPlace)
            {
                case BuildingsManager.Place.ABLE:
                    _currentCursor = cursor.POINTER;
                    break;
                case BuildingsManager.Place.NOT_ABLE:
                    _currentCursor = cursor.NO_BUILDING_IN;
                    break;
            }

        }

        /// <summary>
        /// Checks if the pointd object is an ally
        /// </summary>
        private bool ItsAlly() {
            GameObject _object;
            if ((_object = _inputs.FindHitEntity()))
            {
                return ItsAlly(_object);
            }
            else
                return false;
        }

        /// <summary>
        /// returns if the input object is ally
        /// </summary>
        /// <param name="_object"></param>
        /// <returns></returns>
        private bool ItsAlly(GameObject _object)
        {
            IGameEntity entity = _object.GetComponent<IGameEntity>();
            if (entity.info.race == _player.race)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns if the pointed entity is an enemy
        /// </summary>
        /// <returns></returns>
        private bool ItsEnemy() {
            GameObject _object;
            if ((_object = _inputs.FindHitEntity()))
            {
                return ItsEnemy(_object);
            }
            else
                return false;
        }

        /// <summary>
        /// Returns if the input object is an enemy
        /// </summary>
        /// <param name="_object"></param>
        /// <returns></returns>
        private bool ItsEnemy(GameObject _object)
        {
            IGameEntity entity = _object.GetComponent<IGameEntity>();
            /// if it's dead is not an enemy
            if (entity.info.race != _player.race 
                && entity.status != EntityStatus.DEAD 
                && entity.status != EntityStatus.DESTROYED)
                return true;
            else
                return false;
        }



    }
}
