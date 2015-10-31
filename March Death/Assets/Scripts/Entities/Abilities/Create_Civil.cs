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
        
        res = _gameObject.GetComponent<Resource>(); ;
          
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
    // Best way to check if building is finished is to check if it has the 
    // default unit, which is created just when building becomes usable.
    /// </summary>
    public override bool isUsable
    {
        get
        {
            return 
                
            //BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, _unitInfo.resources.food) &&
            //BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, _unitInfo.resources.wood) &&
            //BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.GOL, _unitInfo.resources.wood) &&
            //BasePlayer.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, _unitInfo.resources.metal)&&
            res.hasDefaultUnit;       
        }
    }
    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
        _enabled = true;
        base.enable(); 
        res.createCivilian(); 

    }
}
