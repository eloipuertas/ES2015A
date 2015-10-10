using System;
using Storage;

class GenericAbility : Ability
{
    private bool _enabled = false;
    public bool isActive
    {
        get
        {
            return _enabled;
        }
    }

    private UnitAbility _info = null;
    public EntityAction info
    {
        get
        {
            return _info;
        }
    }

    public bool isUsable
    {
        get
        {
            return true;
        }
    }

    public GenericAbility(UnitAbility info) { _info = info; }

    public void disable() { _enabled = false; }
    public void enable() { _enabled = true; }
}
