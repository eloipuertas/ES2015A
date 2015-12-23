using System;
using Storage;
using Managers;

using UnityEngine;

public class Create : Ability
{
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
            BasePlayer player = BasePlayer.getOwner(_entity);

            return ResourcesPlacer.get(player).enoughResources(_info) &&
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
                BasePlayer.player.buildings.createBuilding(_info.targetRace, _info.targetBuilding);
				break;
		}

        base.enable();
    }
}
