using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;
using Utils;

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

    /// Precach some actions
    public T DAMAGED { get; set; }
    public T DESTROYED { get; set; }

    private float buildTime1 = 0;
    private float buildTime2 = 0;
    private float buildTime3 = 0;


    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire(DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire(DESTROYED);
    }

    public override IKeyGetter registerFatalWounds(Action<System.Object> func)
    {
        return register(DESTROYED, func);
    }

    public override IKeyGetter unregisterFatalWounds(Action<System.Object> func)
    {
        return unregister(DESTROYED, func);
    }

    /// <summary>
    /// When destroyed, it's called
    /// </summary>
    public override void OnDestroy() 
    {
        ConstructionGrid grid = GameObject.FindGameObjectWithTag("GameController").GetComponent<ConstructionGrid>();
        Vector3 disc_pos = grid.discretizeMapCoords(gameObject.transform.position);
        grid.liberatePosition(disc_pos);

        base.OnDestroy();
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Awake()
    {
        DAMAGED = (T)Enum.Parse(typeof(T), "DAMAGED", true);
        DESTROYED = (T)Enum.Parse(typeof(T), "DESTROYED", true);

        // Call GameEntity start
        base.Awake();

        // Set the status
        setStatus(EntityStatus.BUILDING_PHASE_1);
        activateFOWEntity();
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

        switch (status)
        {
            case EntityStatus.BUILDING_PHASE_1:
                if (buildTime1 >= 3)
                    setStatus(EntityStatus.BUILDING_PHASE_2);
                else
                    buildTime1 += Time.deltaTime;
                break;

            case EntityStatus.BUILDING_PHASE_2:
                if (buildTime2 >= 10)
                    setStatus(EntityStatus.BUILDING_PHASE_3);
                else
                    buildTime2 += Time.deltaTime;
                break;

            case EntityStatus.BUILDING_PHASE_3:
                if (buildTime3 >= 3)
                    setStatus(EntityStatus.IDLE);
                else
                    buildTime3 += Time.deltaTime;
                break;
        }
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }
}
