using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class SelectionManager
    {
        private List<Selectable> _selectedEntities;
        private Storage.Races _ownRace;

        public SelectionManager()
        {
            _selectedEntities = new List<Selectable>();
        }

        /// <summary>
        /// Setter for the race
        /// </summary>
        /// <param name="race"></param>
        public void SetRace(Storage.Races race)
        {
            _ownRace = race;
        }

        /// <summary>
        /// SelectUnique performs a deselection of the current entities, if there are any selected, and a selection of the entity specified by parameter
        /// </summary>
        /// <param name="selectable">The entity that is going to be selected </param>
        public void SelectUnique(Selectable selectable)
        {
            // firstly it checks if an entity can be selected
            if (!CanBeSelected(selectable)) return;

            //Deselect other selected objects
            if (_selectedEntities.Count  > 0) EmptySelection();

            Select(selectable);
            
        }



        /// <summary>
        /// Select performs a simple selection of the entity specified by paramater
        /// </summary>
        /// <param name="selectable">The entity that is going to be selected </param>
        public void Select(Selectable selectable)
        {
            
            // do not select two times the same element
            if (!_selectedEntities.Contains(selectable))
            {
                // Add the entity to the list selects the selectable component
                _selectedEntities.Add(selectable);
                selectable.SelectEntity();
            }
        }


        /// <summary>
        /// Removes the specified entity from the selected entities
        /// </summary>
        /// <param name="entity"></param>
        public void Deselect(Selectable selectable)
        {
            if (_selectedEntities.Contains(selectable))
            {
                _selectedEntities.Remove(selectable);
                selectable.DeselectEntity();
            }


        }

        /// <summary>
        /// Deselect and entity from the selected entitite without notify to the selectable element. This method should be used only from a selectable class
        /// </summary>
        /// <param name="selectable"></param>
        public void DeselectFromSelected(Selectable selectable)
        {
            if (_selectedEntities.Contains(selectable))
            {
                _selectedEntities.Remove(selectable);
            }
        }


        /// <summary>
        /// Emptyes the list of selected units, also notify every unit that is not selected anymore
        /// </summary>
        public void EmptySelection()
        {
            Selectable[] selectedEntities = _selectedEntities.ToArray();
            _selectedEntities.Clear();

            foreach (Selectable selected in selectedEntities)
            {
                selected.DeselectEntity();
            }
           
        }


        /// <summary>
        /// Returns if an entity can be selected, now just checking if it is the same race
        /// Currently
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool CanBeSelected(Selectable selectable)
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
            foreach (Selectable selected in _selectedEntities)
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
            foreach (Selectable selected in _selectedEntities)
            {
                
                if (selected.entity.info.isUnit)
                {
                    Unit unit = selected.GetComponent<Unit>();
                    // so far we only can attack units
                    //TODO : (hermetico) check why we can't attack buildings
                    if(enemy.info.isUnit) unit.attackTarget((Unit)enemy);
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
