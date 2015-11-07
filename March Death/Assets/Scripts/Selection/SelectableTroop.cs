using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Basic Selectable Troop implementation
/// </summary>
public class SelectableTroop
{
    protected List<Selectable> _selectedEntities;
    public int Count { get { return _selectedEntities.Count; } }


    public SelectableTroop()
    {
        _selectedEntities = new List<Selectable>();
    }


    public SelectableTroop(List<Selectable> selectables)
    {
        _selectedEntities = selectables;
    }


    public void ChangeSelectables(List<Selectable> selectables)
    {
        _selectedEntities = selectables;

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
