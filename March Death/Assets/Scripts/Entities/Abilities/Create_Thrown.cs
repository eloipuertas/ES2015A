using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateThrown : CreateUnit
{
    protected override UnitTypes type { get { return UnitTypes.THROWN; } }

    public CreateThrown(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    { }
}
