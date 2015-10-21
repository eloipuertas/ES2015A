using System;
using Storage;

using UnityEngine;

class Create : Ability
{
    private bool _enabled = false;
	private IGameEntity _entity;
	private BuildingInfo _info_tobuild;
    
    protected BuildingTypes _type;


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
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
		GameObject.Find("GameController").GetComponent<BuildingsManager>().createBuilding(_entity.info.race, _type);
     
        _enabled = true;
        base.enable();
    }
}
