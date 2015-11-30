using System;
using Storage;

using UnityEngine;

class ChangeWatchtowerDirection : Ability
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

    public ChangeWatchtowerDirection(EntityAbility info, GameObject gameObject) :
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
        _gameObject.transform.Find("LightHouse-Revealer").GetComponent<LightHouseRevealer>().ToggleDirection();
        //Debug.Log("Doing something here");
        //_enabled = true;
        //base.enable();
    }
}
