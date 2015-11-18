using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateLight : CreateUnit
{
    protected override UnitTypes type { get { return UnitTypes.LIGHT; } }

    public CreateLight(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    { }
}
