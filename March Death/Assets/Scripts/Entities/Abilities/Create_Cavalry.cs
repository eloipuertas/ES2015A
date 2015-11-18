using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateCavalry : CreateUnit
{
    protected override UnitTypes type { get { return UnitTypes.CAVALRY; } }

    public CreateCavalry(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    { }
}
