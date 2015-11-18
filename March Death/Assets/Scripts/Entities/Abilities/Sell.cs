using System;
using Storage;

using UnityEngine;

class Sell : Ability
{
    private bool _enabled = false;
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
            return true;
        }
    }

    public Sell(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }

    public override void disable()
    {
        _enabled = false;
        base.disable();
    }

    public override void enable()
    {
        _gameObject.GetComponent<IGameEntity>().Destroy(true);
        _enabled = true;
        base.enable();
    }
}
