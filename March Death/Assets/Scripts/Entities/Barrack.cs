using System;
using System.Reflection;
using UnityEngine;
using Storage;


public class Barrack : Building<Barrack.Actions>
{
    public enum Actions {CREATED, DAMAGED, DESTROYED, CREATE_UNIT, BUILDING_FINISHED, HEALTH_UPDATED, ADDED_QUEUE};

    private IGameEntity _entity;

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Awake()
    {
        _info = Info.get.of(race, type);

        // Call GameEntity Awake
        base.Awake();
    }

    public override void Start()
    {
        base.Start();

        _entity = this.GetComponent<IGameEntity>();

        ResourcesEvents.get.registerResourceToEvents(_entity);

        if (BasePlayer.player.race.Equals(_info.race) && !type.Equals(BuildingTypes.STRONGHOLD))
            fire(Actions.CREATED, _entity);
    }
}
