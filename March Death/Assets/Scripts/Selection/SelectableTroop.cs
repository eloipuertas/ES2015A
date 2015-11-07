using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class SelectableTroop: ISelection
{
    private List<Selectable> _selectedEntities;
    public int Count { get { return _selectedEntities.Count; } }


    public SelectableTroop()
    {
        _selectedEntities = new List<Selectable>();
    }


    public SelectableTroop(List<Selectable> selectables)
    {
        _selectedEntities = selectables;
    }

    public void Select(Selectable selectable)
    {
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
            selectable.DeselectEntity();
        }
    }

    public void Clear()
    {
        foreach (Selectable selected in _selectedEntities)
        {
            selected.DeselectEntity();
        }
    }

    public void Remove(Selectable selectable)
    {
        selectable.DeselectEntity();
    }


    public bool IsTroop()
    {
        return true;
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
