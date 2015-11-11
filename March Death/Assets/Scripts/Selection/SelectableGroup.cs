using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

public class SelectableGroup : SelectableTroop
{
    public SelectableGroup() : base() { }
    public SelectableGroup(List<Selectable> selectables) : base(selectables) { }


    
    public void Select(Selectable selectable)
    {
        // Add the entity to the list selects the selectable 
        // do not select two times the same element
        //TODO:(hermetico) user assertions and not booleans
        Assert.IsFalse(_selectedEntities.Contains(selectable));
        _selectedEntities.Add(selectable);
        selectable.SelectEntity();

    }

    public void Select(List<Selectable> selectables)
    {
        Clear();
        
        foreach (Selectable selected in _selectedEntities)
        {
            Select(selected);
        }

    }

    public void Deselect(Selectable selectable)
    {
        //TODO:(hermetico) user assertions and not booleans
        Assert.IsTrue(_selectedEntities.Contains(selectable));
        _selectedEntities.Remove(selectable);
        selectable.DeselectEntity();
    }

    public void Clear()
    {
        Selectable[] selectedEntities = _selectedEntities.ToArray();
        _selectedEntities.Clear();

        foreach (Selectable selected in selectedEntities)
        {
            selected.DeselectEntity();
        }
    }

    public void Remove(Selectable selectable)
    {
        _selectedEntities.Remove(selectable);
    }


}

