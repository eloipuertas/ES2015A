using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;
using Storage;

public class GhostBuilding : GameEntity<GhostBuilding.Actions>
{
    public enum Actions { }

    public BuildingTypes type;
    public override E getType<E>() { return (E)Convert.ChangeType(type, typeof(E)); }


    public override EntityStatus DefaultStatus { get; set; }

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

    public GhostBuilding()
    {
        // Do nothing, not even call parent!
    }

    public override void Start()
    {
        // Do nothing, not even call parent!
    }

    public override void Update()
    {
        // Do nothing, not even call parent!
    }

    public override void Awake()
    {
        // Do nothing, not even call parent!
    }

    public override void OnDestroy()
    {
        // Do nothing, not even call parent!
    }
}
