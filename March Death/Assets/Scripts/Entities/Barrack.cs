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
    public override void Awake()
    {
        _info = Info.get.of(race, type);

        // Call GameEntity Awake
        base.Awake();
    }
}
