using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

/// <summary>
/// Basic implementation of a selectable Group, inherits from SelectableTroop
/// </summary>
public class SelectableGroup : SelectableTroop
{
    public SelectableGroup() : base() { }
    public SelectableGroup(List<Selectable> selectables) : base(selectables) { }


    /// <summary>
    /// Adds the input entity to the selected list and calls the select function of the input element
    /// </summary>
    /// <param name="selectable"></param>
    public void Select(Selectable selectable)
    {
        // Add the entity to the list selects the selectable 
        // do not select two times the same element
        Assert.IsFalse(_selectedEntities.Contains(selectable));
        _selectedEntities.Add(selectable);
        selectable.SelectEntity();

    }

    /// <summary>
    /// Clears the current list and adds each element of the input list 
    /// and calls the select function of each element of the input list
    /// </summary>
    /// <param name="selectables"></param>
    public void Select(List<Selectable> selectables)
    {
        Clear();
        
        foreach (Selectable selected in selectables)
        {
            Select(selected);
        }

    }


    /// <summary>
    /// Removes the input element of the current list and calls the deselect function of the selectable input element
    /// If you don't want to call the deselect method of the selectable element that is going to be removed
    /// use the Remove() method of this class
    /// </summary>
    /// <param name="selectable"></param>
    public void Deselect(Selectable selectable)
    {
        //TODO:(hermetico) user assertions and not booleans
        Assert.IsTrue(_selectedEntities.Contains(selectable));
        _selectedEntities.Remove(selectable);
        selectable.DeselectEntity();
    }


    /// <summary>
    /// Clear the current list of selected elements and also calls the deselect  method of each selectable element that was in the
    /// list
    /// </summary>
    public void Clear()
    {
        Selectable[] selectedEntities = _selectedEntities.ToArray();
        _selectedEntities.Clear();

        foreach (Selectable selected in selectedEntities)
        {
            selected.DeselectEntity();
        }
    }


    /// <summary>
    /// Removes the input element of the current list without calling the deselecte function of the selectable inupt
    /// If you want to call that function too, use Deselect() method of this class
    /// </summary>
    /// <param name="selectable"></param>
    public void Remove(Selectable selectable)
    {
        _selectedEntities.Remove(selectable);
    }


}

