using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class SelectableGroup : ISelection
{
    private List<Selectable> _selectedEntities;
    public int Count { get { return _selectedEntities.Count; } }


    public SelectableGroup()
    {
        _selectedEntities = new List<Selectable>();
    }

    public SelectableGroup(List<Selectable> selectables)
    {
        _selectedEntities = selectables;
    }


    /// <summary>
    /// SelectUnique performs a deselection of the current entities, if there are any selected, and a selection of the entity specified by parameter
    /// </summary>
    /// <param name="selectable">The entity that is going to be selected </param>
    public void Select(Selectable selectable)
    {
        // Add the entity to the list selects the selectable 
        // do not select two times the same element
        if (!_selectedEntities.Contains(selectable))
        {
            _selectedEntities.Add(selectable);
            selectable.SelectEntity();
        }

    }

    public void Select(List<Selectable> selectables)
    {
        Clear();
        _selectedEntities = selectables;

    }

    public void Deselect(Selectable selectable)
    {
        if (_selectedEntities.Contains(selectable))
        {
            _selectedEntities.Remove(selectable);
            selectable.DeselectEntity();
        }
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


    public bool IsTroop()
    {
        return false;
    }

    public bool Contains(Selectable selectable)
    {
        return _selectedEntities.Contains(selectable);
    }

    public Selectable[] ToArray()
    {
        return _selectedEntities.ToArray();
    }

    public ArrayList ToArrayList()
    {
        return new ArrayList(_selectedEntities.ToArray());
    }

    public List<Selectable> ToList()
    {
        return new List<Selectable>(_selectedEntities);
    }

}

