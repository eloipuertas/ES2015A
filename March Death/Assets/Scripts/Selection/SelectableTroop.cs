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

    /// <summary>
    /// Creates an object with empty selection
    /// </summary>
    public SelectableTroop()
    {
        _selectedEntities = new List<Selectable>();
    }

    /// <summary>
    /// Creates an object with the input list selection
    /// </summary>
    /// <param name="selectables"></param>
    public SelectableTroop(List<Selectable> selectables)
    {
        _selectedEntities = selectables;
    }

    /// <summary>
    /// Modifies the current selection
    /// </summary>
    /// <param name="selectables"></param>
    public void ChangeSelectables(List<Selectable> selectables)
    {
        _selectedEntities = selectables;

    }

    /// <summary>
    /// Returns if the current list contains the specified selectable object
    /// </summary>
    /// <param name="selectable"></param>
    /// <returns></returns>
    public bool Contains(Selectable selectable)
    {
        return _selectedEntities.Contains(selectable);
    }

    /// <summary>
    /// Returns an array of the current list
    /// </summary>
    /// <returns></returns>
    public Selectable[] ToArray()
    {
        return _selectedEntities.ToArray();
    }


    /// <summary>
    /// Returns an array list of the current list
    /// </summary>
    /// <returns></returns>
    public ArrayList ToArrayList()
    {
        return new ArrayList(_selectedEntities.ToArray());
    }


    /// <summary>
    /// Returns a copy of the current list
    /// </summary>
    /// <returns></returns>
    public List<Selectable> ToList()
    {
        return new List<Selectable>(_selectedEntities);
    }

}
