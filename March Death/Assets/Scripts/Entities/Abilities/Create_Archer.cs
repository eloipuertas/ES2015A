using Storage;
using UnityEngine;
using System.Collections;

public class CreateArcher : Ability
{
    public Archery archery;
    private GameObject Archer;
    private bool _enabled = false;
    private IGameEntity _entity;

    public override bool isActive
    {
        get
        {
            return _enabled;
        }
    }

    public CreateArcher(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
        archery = _gameObject.GetComponent<Archery>();
    }


    /// <summary>
    /// Ability is not usable if player hasn't enough materials to spend in 
    /// unit construction or queue is Full.
    // Best way to check if building is finished is to check building status.
    /// </summary>
    public override bool isUsable
    {
        get
        {

            UnitInfo unitInfo;
            unitInfo = Info.get.of(archery.race, UnitTypes.ARCHER);

            return
            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, unitInfo.resources.food) &&
            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, unitInfo.resources.metal) &&
            Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, unitInfo.resources.wood) &&
            //Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.GOLD, unitInfo.resources.gold) &&
            (archery.status == EntityStatus.IDLE);

        }
    }
    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {

        base.enable();

        switch(archery.buttonNewArcherStatus)
        {
            case (Archery.createArcherStatus.DISABLED):
                Debug.Log(" OPTION IS DISABLED "); 
                break;
            case (Archery.createArcherStatus.IDLE):
                archery.createArcher();
                break;
            case (Archery.createArcherStatus.RUN):
                archery.createArcher();
                break;
            case (Archery.createArcherStatus.FULL):
                break;
        }  
    }
}
