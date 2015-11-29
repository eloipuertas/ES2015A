
using System;
using Storage;
using Managers;

using UnityEngine;

public class RecruitExplorer : Ability
{

    private IGameEntity _entity;
    private EntityInfo _entityInfo;
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
            return (_gameObject.GetComponent<Resource>().harvestUnits > 0) &&
                (_entity.status == EntityStatus.IDLE || _entity.status == EntityStatus.WORKING);
            
        }
    }

    public RecruitExplorer(EntityAbility info, GameObject gameObject) : base(info, gameObject)
    {
        _entity = _gameObject.GetComponent<IGameEntity>();
    }

    public override void disable()
    {
        base.disable();
    }

    public override void enable()
    {
        _gameObject.GetComponent<Resource>().recruitExplorer();
        base.enable();
    }
}


