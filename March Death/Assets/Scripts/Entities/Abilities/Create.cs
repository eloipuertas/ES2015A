using System;
using Storage;
using Managers;

using UnityEngine;

public class Create : Ability
{
	private static BuildingsManager BuildingsMgr = null;

	private IGameEntity _entity;
	private EntityInfo _infoToBuild;

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
            return BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, _infoToBuild.resources.food) &&
					BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, _infoToBuild.resources.wood) &&
					BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, _infoToBuild.resources.metal) &&
					(_entity.status == EntityStatus.IDLE || _entity.status == EntityStatus.WORKING);
        }
    }

    public Create(EntityAbility info, GameObject gameObject) : base(info, gameObject)
    {
		_entity = _gameObject.GetComponent<IGameEntity>();

		switch (_info.targetType)
		{
			case EntityType.UNIT:
				_infoToBuild = Info.get.of(_info.targetRace, _info.targetUnit);
				break;

			case EntityType.BUILDING:
				_infoToBuild = Info.get.of(_info.targetRace, _info.targetBuilding);
				break;
		}

		if (BuildingsMgr == null)
		{
			BuildingsMgr = GameObject.Find("GameController").GetComponent<Main_Game>().BuildingsMgr;
		}
    }

    public override void disable()
    {
        base.disable();
    }

    public override void enable()
    {
		switch (_info.targetType)
		{
			case EntityType.UNIT:
                // Resource is building too. We need check if it is resource because Resource 
                // script must control when new unit is added to queue or not.
                // Please be careful if you need to change this.
                if (_entity.info.isResource)
                {
                    _gameObject.GetComponent<Resource>().newCivilian();
                }
                else
                {
                    _entity.doIfBuilding(building => building.addUnitQueue(_info.targetUnit));
                }
				
				break;

            case EntityType.BUILDING:
				BuildingsMgr.createBuilding(_info.targetRace, _info.targetBuilding);
				break;
		}

        base.enable();
    }
}
