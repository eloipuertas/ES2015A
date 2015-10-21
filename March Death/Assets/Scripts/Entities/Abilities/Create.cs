using System;
using Storage;

using UnityEngine;

class Create : Ability
{
    private bool _enabled = false;
    private IGameEntity _entity;
    
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
            //Check resources
            return true;
        }
    }

    public Create(EntityAbility info, GameObject gameObject) : base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
    }

    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
        GameObject.Find("GameController").createBuilding(_entity.info.race, _type);
     
        _enabled = true;
        base.enable();
    }
}
