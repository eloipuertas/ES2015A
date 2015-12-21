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

        if (resources != null) {
			BasePlayer player = BasePlayer.getOwner (entity);
			ResourcesPlacer.get (player).Collect (WorldResources.Type.FOOD, resources.food);
			ResourcesPlacer.get (player).Collect (WorldResources.Type.WOOD, resources.wood);
			ResourcesPlacer.get (player).Collect (WorldResources.Type.METAL, resources.metal);
		} else {
			Debug.LogWarning (entity + " Have no sellValue on JSON");
		}
		BasePlayer.player.selection.deselectBuilding();
        _gameObject.GetComponent<IGameEntity>().Destroy(true);
        _enabled = true;
		base.enable();
    }
}
