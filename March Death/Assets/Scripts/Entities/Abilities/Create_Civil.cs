using UnityEngine;
using System.Collections;
using System;
using Storage;

public class CreateCivil : CreateUnit
{
    protected override UnitTypes type { get { return UnitTypes.CIVIL; } }

    public CreateCivil(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    { }
}
