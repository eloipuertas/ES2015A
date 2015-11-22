using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateHeavy : CreateUnit
{
    protected override UnitTypes type { get { return UnitTypes.HEAVY; } }

    public CreateHeavy(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    { }
}
