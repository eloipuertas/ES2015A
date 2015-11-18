using System;
using Storage;
using Managers;

using UnityEngine;

abstract class Create : Ability
{
	private IGameEntity _entity;
	private BuildingInfo _info_tobuild;
    
    protected abstract BuildingTypes _type { get; }


    public override bool isActive
    {
        get
        {
            return false;
        }
    }

    public override bool isUsable
    {
        get
        {
			return BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, _info_tobuild.resources.food) &&
					BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, _info_tobuild.resources.wood) &&
					BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, _info_tobuild.resources.metal);            
        }
    }

    public Create(EntityAbility info, GameObject gameObject) : base(info, gameObject)
    {
		_entity = _gameObject.GetComponent<IGameEntity>();
		_info_tobuild = Info.get.of(_entity.info.race, _type);
    }

    public override void disable()
    {
        base.disable();
    }

    public override void enable()
    {
		GameObject.Find("GameController").GetComponent<Main_Game>().BuildingsMgr.createBuilding(_entity.info.race, _type);
        base.enable();
    }
}
