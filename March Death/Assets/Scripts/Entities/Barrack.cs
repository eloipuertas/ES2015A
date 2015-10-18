using System;
using System.Reflection;
using UnityEngine;
using Storage;


public class Barrack : Building<Barrack.Actions>
{
	public enum Actions {DAMAGED, DESTROYED};

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        _info = Info.get.of(race, type);

        // Call GameEntity start
        base.Start();
    }
}
