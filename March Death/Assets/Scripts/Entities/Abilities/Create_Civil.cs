using System;
using Storage;
using Managers;
using UnityEngine;

class CreateCivil : Ability
{
    private bool _enabled = false;
    private IGameEntity _entity;
    private UnitInfo _unitInfo;
    public Resource res;

    public CreateCivil(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
        Resource res = (Resource)_entity;
          
    }

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
            
            return BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, _unitInfo.resources.food) &&
                   BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, _unitInfo.resources.wood) &&
                   //BasePlayer..getOwner(_entity).resources.IsEnough(WorldResources.Type.GOL, _unitInfo.resources.wood) &&
                   BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, _unitInfo.resources.metal);
                   
        }
    }
    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {

        GameObject.Find("GameController").GetComponent<ResourcesManager>().payCivilUnit(_entity.info.race, UnitTypes.CIVIL);
        _enabled = true;
        base.enable();
        res.createCivilian();// Debugging, not functional
        
    }
}
