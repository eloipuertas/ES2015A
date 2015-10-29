using System;
using Storage;
using Managers;
using UnityEngine;

class CreateCivil : Ability
{
    private IGameEntity _entity;
    private UnitInfo _unitInfo;
    public Resource res;

    public CreateCivil(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
        
        res = (Resource)_entity;
          
    }

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
            // DEBUGGING 
            return true;
            /*
            // we need to check if player has enough materials to spend in building construction
            return BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, _unitInfo.resources.food) &&
                   BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, _unitInfo.resources.wood) &&
                   //BasePlayer..getOwner(_entity).resources.IsEnough(WorldResources.Type.GOL, _unitInfo.resources.wood) &&
                   BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, _unitInfo.resources.metal);
             */
        }
    }
    public override void disable()
    {
        base.disable();
    }

    public override void enable()
    {
        base.enable(); 
        res.createCivilian(); 

    }
}
