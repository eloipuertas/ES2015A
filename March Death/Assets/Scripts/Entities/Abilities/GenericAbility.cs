using System;
using Storage;

using UnityEngine;

class GenericAbility : Ability
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

    public GenericAbility(UnitAbility info, GameObject gameObject) :
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
        _enabled = true;
        base.enable();
    }
}
