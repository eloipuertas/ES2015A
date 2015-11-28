using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

class Squad : GameEntity<Squad.Actions>
{
    private EntityStatus _defaultStatus = EntityStatus.IDLE;
    public override EntityStatus DefaultStatus
    {
        get
        {
            return _defaultStatus;
        }
        set
        {
            _defaultStatus = value;
        }
    }

    public override E getType<E>() { throw new NotImplementedException(); }

    public override IKeyGetter registerFatalWounds(Action<object> func)
    {
        throw new NotImplementedException();
    }

    public override IKeyGetter unregisterFatalWounds(Action<object> func)
    {
        throw new NotImplementedException();
    }

    protected override void onFatalWounds()
    {
        throw new NotImplementedException();
    }

    protected override void onReceiveDamage()
    {
        throw new NotImplementedException();
    }

    public enum Actions { }


}

