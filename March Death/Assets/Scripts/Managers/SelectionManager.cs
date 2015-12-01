using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Managers
{
    public class SelectionManager : SubscribableActor<SelectionManager.Actions, SelectionManager>
    {
        public enum Actions { SELECT, ATTACK, MOVE};

        // class controller for the selected entities
        private Squad _selectedSquad;
        public Squad SelectedSquad
        {
            get
            {
                return _selectedSquad;
            }
        }

        // Troops
        private Dictionary<string, Squad> _troops = new Dictionary<string, Squad>();

        // Do we have squads or buildings?
        private bool _isSquad = true;
        public bool IsQuad { get { return _isSquad; } }
        private IBuilding _selectedBuilding;

        // the own race
        private Storage.Races _ownRace;

        // to know whether the current selection is a troop or is just a bunch of selected entities
        private bool _isTroop = false;
        public bool IsTroop { get { return _isTroop; } }

        // the amount of troops made
        public int TroopsCount { get { return _troops.Count; } }

        public bool IsUnique
        {
            get
            {
                return (!_isSquad && _selectedBuilding != null) || (_selectedSquad != null && _selectedSquad.Units.Count == 1);
            }
        }

        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Setter for the race
        /// </summary>
        /// <param name="race"></param>
        public void SetRace(Storage.Races race)
        {
            _ownRace = race;
        }

        // TODO: Should be optimized out
        public bool IsInTroop(Unit unit)
        {
            foreach (var entry in _troops)
            {
                if (entry.Value.Units.Contains(unit))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new troop from the currently selected elements.
        /// Returns true if the troop is created succesfully , returns false if not.
        /// A troop should have more than one element, if not, will return false
        /// </summary>
        /// <param name="key">a key for the troop</param>
        /// <returns></returns>
        public bool NewTroop(String key)
        {
            Assert.IsFalse(_troops.ContainsKey(key));

            if (_selectedSquad.Units.Count > 1)
            {
                // Set unit squad for fast access
                foreach (Unit unit in _selectedSquad.Units)
                {
                    // Is it already in another troop?
                    foreach (var entry in _troops)
                    {
                        if (entry.Value.Units.Contains(unit))
                        {
                            entry.Value.RemoveUnit(unit);
                        }
                    }

                    unit.Squad = _selectedSquad;
                }

                _troops.Add(key, _selectedSquad);

                Debug.Log("Created troop: " + key);
                return true;
            }
            else
            {
                Debug.Log("Troops should have more than 1 unit");
                return false;
            }
        }


        /// <summary>
        /// Select performs a simple selection of the entity specified by paramater
        /// </summary>
        /// <param name="selectable">The entity that is going to be selected </param>
        public void Select(IGameEntity entity)
        {
            Assert.IsTrue(CanBeSelected(entity));

            _isSquad = entity.info.isUnit;
            if (_isSquad)
            {
                SelectSquad(((Unit)entity).Squad);
            }
            else
            {
                _selectedBuilding = (IBuilding)entity;

                Selectable selectable = _selectedBuilding.getGameObject().GetComponent<Selectable>();
                selectable.SelectEntity();
                fire(Actions.SELECT, selectable);
            }
        }

        public void SelectSquad(Squad squad)
        {
            // If we have no selected squad, use the provided one
            if (_selectedSquad == null)
            {
                _selectedSquad = squad;
            }

            foreach (Unit unit in squad.Units)
            {
                // If we were adding to a troop but the new incoming unit is not of that troop
                // Simply create a new temporary troop
                if (_troops.ContainsValue(_selectedSquad) && _selectedSquad != unit.Squad)
                {
                    // TODO: Doing so will make the squad never update :(
                    Squad temp = new Squad(BasePlayer.player.race);
                    temp.AddUnits(_selectedSquad);
                    _selectedSquad = temp;
                }

                // Add the unit to this crowd (if not already in)
                _selectedSquad.AddUnit(unit);
                //unit.Squad = _selectedSquad;

                // Select the entity
                Selectable selectable = unit.GetComponent<Selectable>();
                selectable.SelectEntity();

                // Fire selected
                fire(Actions.SELECT, selectable);
            }
        }

        /// <summary>
        /// Deselects the specified entity from the selected entities
        /// </summary>
        /// <param name="entity"></param>
        public void Deselect(IGameEntity entity)
        {
            // Avoid getting AI calls
            if (entity.info.race != BasePlayer.player.race)
            {
                return;
            }

            // In case we are doing units
            if (entity.info.isUnit)
            {
                Squad squad = _selectedSquad != null ? _selectedSquad : ((Unit)entity).Squad;

                // Deselect all units in the unit squad
                foreach (Unit unit in squad.Units)
                {
                    // Deselect entity
                    unit.GetComponent<Selectable>().DeselectEntity();

                    // Unless this unit is part of a squad saved as a troop, null it out
                    if (!_troops.ContainsValue(unit.Squad))
                    {
                        unit.Squad = null;
                    }
                }

                // Deselect current squad
                _selectedSquad = null;
            }
            else if (entity == _selectedBuilding)
            {
                // Deselect current building
                _selectedBuilding.getGameObject().GetComponent<Selectable>().DeselectEntity();
                _selectedBuilding = null;
            }
        }

        public void DeselectCurrent()
        {
            if (_isSquad && _selectedSquad != null && _selectedSquad.Units.Count > 0)
            {
                Deselect(_selectedSquad.Units[0]);
            }
            else if (!_isSquad && _selectedBuilding != null)
            {
                Deselect(_selectedBuilding);
            }
        }


        /// <summary>
        /// Checks if there is a troop created with the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasTroop(string key)
        {
            return _troops.ContainsKey(key);
        }


        /// <summary>
        /// Performs a selection of the troop related to the provided key
        /// The key must exist, please use the methid HasTroop prior to call this method.
        /// It changes the current selection for the elements in the troop, also calls each select function
        /// of each selectable element.
        /// Also changes the state of this manager to show that the current selection is a troop
        /// </summary>
        /// <param name="key"></param>
        public void SelectTroop(string key)
        {
            Assert.IsTrue(_troops.ContainsKey(key));

            // Deselect current selection
            DeselectCurrent();

            // Select troop
            Squad selected = _troops[key];
            SelectSquad(selected);

            Debug.Log("Selected troop: " + key);
        }

        /// <summary>
        /// Returns if an entity can be selected, now just checking if it is the same race
        /// Currently
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool CanBeSelected(IGameEntity entity)
        {
            return _ownRace == entity.getRace();
        }

        /// <summary>
        /// Returns if there are selected entities
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return _selectedSquad.Units.Count > 0;
        }

        /// <summary>
        /// Deletes the troop specified by parameter.
        /// First it checks if the current selection is a troop. This ehaviour may change
        /// </summary>
        /// <param name="key"></param>
        public void DeleteTroop(string key)
        {
            _troops.Remove(key);
        }

        /// <summary>
        /// Removes all the buildings from the current selection
        /// </summary>
        [Obsolete]
        public void RemoveBuildinds()
        {
            /*
            foreach (Selectable selected in _selectedEntities.ToArray())
            {
                if (selected.entity.info.isBuilding)
                {
                    selected.DeselectEntity();
                    _selectedEntities.Remove(selected);

                }
            }
            */
        }


        /// <summary>
        /// Basic movement operation for the selected entities
        /// </summary>
        /// <param name="point"></param>
        public void MoveTo(Vector3 point)
        {
            _selectedSquad.MoveTo(point, unit => fire(Actions.MOVE, unit));
            Debug.Log("Moving there");
        }



        /// <summary>
        /// Basic attack operation for the selected entities
        /// </summary>
        /// <param name="point"></param>
        public void AttackTo(IGameEntity enemy)
        {
            _selectedSquad.AttackTo(enemy, unit => fire(Actions.ATTACK, unit));
            Debug.Log("attacking");
        }

        /// <summary>
        /// Entering resource building. only civilians units are able to do this 
        /// </summary>
        /// 
        public void Enter(IGameEntity building_resource)
        {
            _selectedSquad.EnterTo(building_resource, unit => fire(Actions.MOVE, unit));
            Debug.Log("Walking to building");
        }

        /// <summary>
        /// This method checks if the current selection is a building, returns true or false.
        /// </summary>
        /// <returns></returns>
        public bool IsBuilding()
        {
            //there aren't buildings in multiple selection
            /*
            if (_selectedEntities.Count == 1)
            {
                if(_selectedEntities.ToArray()[0].entity.info.isBuilding) return true;
            }
            */
            return false;
        }
    }
}
