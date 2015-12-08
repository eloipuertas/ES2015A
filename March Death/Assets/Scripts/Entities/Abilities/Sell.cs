using System;
using Storage;

using UnityEngine;

class Sell : Ability
{
    private bool _enabled = false;
    public override bool isActive
    {
        get
        {
            return _enabled;
        }
    }

    public override bool isUsable
    {
        get
        {
            return true;
        }
    }

    public Sell(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }

    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
        // Give resources back
        IGameEntity entity = _gameObject.GetComponent<IGameEntity>();
        EntityResources resources = entity.info.buildingAttributes.sellValue;

        BasePlayer.getOwner(entity).resources.AddAmount(WorldResources.Type.WOOD, resources.wood);
        BasePlayer.getOwner(entity).resources.AddAmount(WorldResources.Type.METAL, resources.metal);
        BasePlayer.getOwner(entity).resources.AddAmount(WorldResources.Type.FOOD, resources.food);

        // Destroy it (onDestroy will handle grid freeing)
        _gameObject.GetComponent<IGameEntity>().Destroy(true);
        _enabled = true;
        base.enable();
    }
}
