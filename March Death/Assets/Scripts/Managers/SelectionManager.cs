using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Managers
{
    public class SelectionManager : Actor<SelectionManager.Actions>
    {
        public enum Actions { SELECT, ATTACK, MOVE};
        // class controller for the selected entities
        private SelectableGroup _selectedEntities = new SelectableGroup();
        //Troops
        private Dictionary<string, SelectableTroop> _troops = new Dictionary<string, SelectableTroop>();
        // the own race
        private Storage.Races _ownRace;
        // to know whether the current selection is a troop or is just a bunch of selected entities
        private bool _isTroop = false;
        public bool IsTroop { get { return _isTroop; } }

        // the amount of troops made
        public int TroopsCount { get { return _troops.Count; } }

        /// <summary>
        /// Setter for the race
        /// </summary>
        /// <param name="race"></param>
        public void SetRace(Storage.Races race)
        {
            _ownRace = race;
        }


        /// <summary>
        /// This method selects just an element, if there are more elements in the selected list, it will remove them
        /// Also changes the stat of the selection to a not troop selection
        /// </summary>
        /// <param name="selectable"></param>
        public void SelectUnique(Selectable selectable)
        {
            // firstly it checks if an entity can be selected
            //if (!CanBeSelected(selectable)) return;
            Assert.IsTrue(CanBeSelected(selectable));


            _isTroop = false;

             if( _selectedEntities.Count > 0 ) _selectedEntities.Clear();

            _selectedEntities.Select(selectable);
            fire(Actions.SELECT, selectable);


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
            if(_selectedEntities.Count > 1)
            {

                SelectableTroop troop = new SelectableTroop(_selectedEntities.ToList());
                _troops.Add(key, troop);
                _isTroop = true;
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
        public void Select(Selectable selectable)
        {
            if (!_selectedEntities.Contains(selectable))
            {
                _selectedEntities.Select(selectable);
                fire(Actions.SELECT, selectable);
                _isTroop = false;
            }
        }


        /// <summary>
        /// Deselects the specified entity from the selected entities
        /// </summary>
        /// <param name="entity"></param>
        public void Deselect(Selectable selectable)
        {
            if (_selectedEntities.Contains(selectable))
            {
                _selectedEntities.Deselect(selectable);
                _isTroop = false;
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

            List<Selectable> selected = _troops[key].ToList();

            _selectedEntities.Select(selected);
            
            foreach(Selectable selectable in selected)
                fire(Actions.SELECT, selectable);

            _isTroop = true;
            Debug.Log("Selected troop: " + key);
        }



        /// <summary>
        /// Deselect and entity from the selected entitite without notify to the selectable element. This method should be used only from a selectable object
        /// </summary>
        /// <param name="selectable"></param>
        public void DeselectFromSelected(Selectable selectable)
        {
            if(_selectedEntities.Contains(selectable))
                _selectedEntities.Remove(selectable);
        }


        /// <summary>
        /// Emptyes the list of selected units, also notify every unit that is not selected anymore
        /// </summary>
        public void EmptySelection()
        {
            _selectedEntities.Clear();
            _isTroop = false;
        }


        /// <summary>
        /// Returns if an entity can be selected, now just checking if it is the same race
        /// Currently
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool CanBeSelected(Selectable selectable)
        {
            return _ownRace == selectable.entity.getRace();
        }


        /// <summary>
        /// Returns if is the unique selected element
        /// </summary>
        /// <param name="selectable"></param>
        /// <returns></returns>
        public bool UniqueSelected(Selectable selectable)
        {
            return _selectedEntities.Count == 1 && _selectedEntities.Contains(selectable);
        }

        /// <summary>
        /// Returns if there are selected entities
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return _selectedEntities.Count > 0;
        }

        /// <summary>
        /// Deletes the troop specified by parameter.
        /// First it checks if the current selection is a troop. This ehaviour may change
        /// </summary>
        /// <param name="key"></param>
        public void DeleteTroop(string key)
        {
            if(_isTroop) EmptySelection();
            _troops.Remove(key);
        }

        /// <summary>
        /// Removes all the buildings from the current selection
        /// </summary>
        public void RemoveBuildinds()
        {
            foreach (Selectable selected in _selectedEntities.ToArray())
            {
                if (selected.entity.info.isBuilding)
                {
                    selected.DeselectEntity();
                    _selectedEntities.Remove(selected);

                }
            }
        }


        /// <summary>
        /// Basic movement operation for the selected entities
        /// </summary>
        /// <param name="point"></param>
        public void MoveTo(Vector3 point)
        {
            GameObject banner = SelectionDestination.CreateBanner(_ownRace);
            Selectable[] units = _selectedEntities.ToArray();
            banner.GetComponent<SelectionDestination>().Deploy(units, point);
            foreach (Selectable selected in units)
            {
                if (selected.entity.info.isUnit)
                {
                    selected.GetComponent<Unit>().moveTo(point);
                    fire(Actions.MOVE, selected);
                }

            }
            
            Debug.Log("Moving there");

        }



        /// <summary>
        /// Basic attack operation for the selected entities
        /// </summary>
        /// <param name="point"></param>
        public void AttackTo(IGameEntity enemy)
        {
            foreach (Selectable selected in _selectedEntities.ToArray())
            {

                if (selected.entity.info.isUnit)
                {
                    Unit unit = selected.GetComponent<Unit>();
                    unit.attackTarget(enemy);
                    fire(Actions.ATTACK, selected);
                }
            }
            Debug.Log("attacking");
        }


        /// <summary>
        /// This method checks if the current selection is a building, returns true or false.
        /// </summary>
        /// <returns></returns>
        public bool IsBuilding()
        {
            //there aren't buildings in multiple selection
            if (_selectedEntities.Count == 1)
            {
                if(_selectedEntities.ToArray()[0].entity.info.isBuilding) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an array list with the current selected entities
        /// </summary>
        /// <returns></returns>
        public ArrayList ToArrayList()
        {
            return new ArrayList(_selectedEntities.ToArray());
        }
    }
}
