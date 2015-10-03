using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;


/// <summary>
/// Building base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Building : Utils.Actor<Building.Actions>, IGameEntity
{
    public enum Actions { DAMAGED, DESTROYED };

    public Building() { }


    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }

    /// <summary>    
    /// Returns NULL as this cannot be converted to Unit
	/// </summary>
    /// <returns>Object casted to Unit</returns>
    public Unit toUnit() { return null;  }

    /// <summary>
    /// Casts this IGameEntity to Unity (pointless if already building)
    /// </summary>
    /// <returns>Always null</returns>
    public Building toBuilding() { return this; }

}
