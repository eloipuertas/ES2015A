using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;


/// <summary>
/// Building base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public abstract class Building<T> : GameEntity<T> where T : struct, IConvertible
{
    public Building() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public BuildingTypes type = BuildingTypes.STRONGHOLD;
    public override E getType<E>() { return (E)Convert.ChangeType(type, typeof(E)); }

    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire((T) Enum.Parse(typeof(T), "DAMAGED", true));
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire((T) Enum.Parse(typeof(T), "DESTROYED", true));
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        // Call GameEntity start
        base.Start();
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }
}
