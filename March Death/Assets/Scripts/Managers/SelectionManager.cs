using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class SelectionManager
    {
        private ISelection _selectedEntities;
        private List<Selectable> _selected;
        private Dictionary<string, ISelection> _troops;
        private Storage.Races _ownRace;
        public int Troops { get { return _troops.Count; } }

        public SelectionManager()
        {

            _selectedEntities = new SelectableGroup();
        }

        /// <summary>
        /// Setter for the race
        /// </summary>
        /// <param name="race"></param>
        public void SetRace(Storage.Races race)
        {
            _ownRace = race;
        }


        public void SelectUnique(Selectable selectable)
        {
            // firstly it checks if an entity can be selected
            if (!CanBeSelected(selectable)) return;

            //Deselects other selected objects
            if (_selectedEntities.Count > 0) _selectedEntities.Clear();

            // Creates new group
            if (_selectedEntities.IsTroop()) _selectedEntities = new SelectableGroup();

            _selectedEntities.Select(selectable);
            
        }

        public void NewTroop(String key)
        {
            SelectableTroop troop = new SelectableTroop(_selectedEntities.ToList());
            _troops.Add(key, troop);
        }


        /// <summary>
        /// Select performs a simple selection of the entity specified by paramater
        /// </summary>
        /// <param name="selectable">The entity that is going to be selected </param>
        public void Select(Selectable selectable)
        {

            // Creates new group
            if (_selectedEntities.IsTroop())
            { 
                _selectedEntities.Clear();
                _selectedEntities = new SelectableGroup();

            }

            _selectedEntities.Select(selectable);

        }


        /// <summary>
        /// Deselects the specified entity from the selected entities
        /// </summary>
        /// <param name="entity"></param>
        public void Deselect(Selectable selectable)
        {
            _selectedEntities.Deselect(selectable);
        }


        public void SelectTroop(string key)
        {
            _selectedEntities.Clear();
            _selectedEntities = _troops[key];
        }



        /// <summary>
        /// Deselect and entity from the selected entitite without notify to the selectable element. This method should be used only from a selectable object
        /// </summary>
        /// <param name="selectable"></param>
        public void DeselectFromSelected(Selectable selectable)
        {
                _selectedEntities.Remove(selectable);
        }


        /// <summary>
        /// Emptyes the list of selected units, also notify every unit that is not selected anymore
        /// </summary>
        public void EmptySelection()
        {
            _selectedEntities.Clear();
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
        /// Returns if there are selected entities
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return _selectedEntities.Count > 0;
        }


        public void DeleteTroop(string key)
        {
            _troops[key].Clear();
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
            foreach (Selectable selected in _selectedEntities.ToArray())
            {
                if (selected.entity.info.isUnit)
                    selected.GetComponent<Unit>().moveTo(point);
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
                    if (enemy.info.isUnit) unit.attackTarget((Unit)enemy);
                    else if(enemy.info.isBarrack)unit.attackTarget((Barrack)enemy);
                    else if(enemy.info.isResource)unit.attackTarget((Resource)enemy);
                }
            }
            Debug.Log("attacking");
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
