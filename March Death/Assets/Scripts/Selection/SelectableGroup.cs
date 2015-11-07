using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class SelectableGroup : SelectableTroop
{
    public SelectableGroup() : base() { }

    public SelectableGroup(List<Selectable> selectables) : base(selectables) { }


    
    public bool Select(Selectable selectable)
    {
        // Add the entity to the list selects the selectable 
        // do not select two times the same element
        if (!_selectedEntities.Contains(selectable))
        {
            _selectedEntities.Add(selectable);
            selectable.SelectEntity();
            return true;
        }
        return false;

    }

    public void Select(List<Selectable> selectables)
    {
        Clear();
        _selectedEntities = selectables;
        foreach (Selectable selected in _selectedEntities)
        {
            selected.SelectEntity();
        }

    }

    public bool Deselect(Selectable selectable)
    {
        if (_selectedEntities.Contains(selectable))
        {
            _selectedEntities.Remove(selectable);
            selectable.DeselectEntity();
            return true;
        }
        return false;
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

