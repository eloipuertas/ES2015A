using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utils;
using Storage;

using UnityEngine;

public abstract class Ability : Actor<Ability.Actions>
{
    public enum Actions { ENABLED, DISABLED };

    protected GameObject _gameObject = null;
    protected EntityAbility _info = null;

    public T info<T>() where T: EntityAbility
    {
        return (T)_info;
    }

    protected Ability(EntityAbility info, GameObject gameObject)
    {
        _info = info;
        _gameObject = gameObject;
    }

    public abstract bool isActive { get; }
    public abstract bool isUsable { get; }

    public virtual void enable()
    {
        fire(Actions.ENABLED, this);
    }

    public virtual void disable()
    {
        fire(Actions.DISABLED, this);
    }

    public void Update() {}

    public void toggle()
    {
        if (isActive)
        {
            disable();
        }
        else
        {
            enable();
        }
    }
}
