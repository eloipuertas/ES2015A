using System;
using Storage;

class GenericBuildingAbility : IBuildingAbility
{
    private bool _enabled = false;
    public bool isActive
    {
        get
        {
            return _enabled;
        }
    }

    private BuildingAbility _info = null;
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

    public GenericBuildingAbility(BuildingAbility info) { _info = info; }

    public void disable() { _enabled = false; }
    public void enable() { _enabled = true; }
    public void toggle()
    {
        if (_enabled)
        {
            disable();
        }
        else
        {
            enable();
        }
    }

    public T getModifier<T>(BuildingModifier modifier)
    {
        if (!_enabled)
        {
            return (T)Convert.ChangeType(0, typeof(T));
        }

        switch (modifier)
        {
            case BuildingModifier.RESISTANCE: return (T)Convert.ChangeType(_info.resistanceModifier, typeof(T));
            case BuildingModifier.WOUNDS: return (T)Convert.ChangeType(_info.woundsModifier, typeof(T));
            default: throw new ArgumentException("Modifier " + modifier + " not found");
        }
    }
}
