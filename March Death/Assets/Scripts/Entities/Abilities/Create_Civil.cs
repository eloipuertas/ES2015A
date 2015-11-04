using System;
using Storage;
using Managers;
using UnityEngine;

class CreateCivil : Ability
{
    private IGameEntity _entity;
    public Resource res;
    private GameObject civil;

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
            return false;
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

            UnitInfo unitInfo;
            unitInfo = Info.get.of(res.race, UnitTypes.CIVIL);

            //Debug.Log("*********   harvestUnits: " + res.harvestUnits);
            //Debug.Log("*********   Building status: " + res.status.ToString());
            //Debug.Log("*********   Create civilian status: " + res.status.ToString());

            return 

            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, unitInfo.resources.food) &&
            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, unitInfo.resources.metal) &&
            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, unitInfo.resources.wood) &&
             //Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.GOLD, unitInfo.resources.gold) &&
            res.hasDefaultUnit;
              
        }
    }
    public override void disable()
    {
        base.disable();
    }

    public override void enable()

    {
        
        base.enable();

        if (res.buttonCivilStatus == Resource.createCivilStatus.IDLE)
        {
            res.createCivilian();
        }
        else if (res.buttonCivilStatus == Resource.createCivilStatus.IDLE)
        {
            Debug.Log("Wait please, assembling CIVIL unit");
        }
        else if (res.buttonCivilStatus == Resource.createCivilStatus.DISABLED)
        {
            Debug.Log(" OPTION IS DISABLED ");
        }


    }
}
