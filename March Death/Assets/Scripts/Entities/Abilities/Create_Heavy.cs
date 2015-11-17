using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateHeavy : Ability
{
    private bool _enabled = false;
    private IGameEntity _entity;
    private GameObject cavalry;
    private UnitInfo unitInfo;

    public CreateHeavy(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
        unitInfo = Info.get.of(_entity.getRace(), UnitTypes.HEAVY);
    }

    public override bool isActive
    {
        get
        {
            return _enabled;
        }
    }
    /// <summary>
    /// Ability is not usable if player hasn't enough materials to spend in 
    /// unit construction or build is under construction.
    // Best way to check if building is finished is to check building status.
    /// </summary>
    public override bool isUsable
    {
        get
        {
            return

                Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, unitInfo.resources.food) &&
                Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, unitInfo.resources.metal) &&
                Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, unitInfo.resources.wood) &&
                _entity.status == EntityStatus.IDLE;
        }
    }
    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
        base.enable();

        if (_entity.info.isBarrack)
        {
            ((Barrack)_entity).addUnitQueue(UnitTypes.HEAVY);
        }
        else
        {
            Debug.Log("Heavy Units must ber generated only at Barracks Building");
        }
    }
}
