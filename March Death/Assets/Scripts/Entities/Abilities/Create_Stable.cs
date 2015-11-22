using System;
using Storage;

using UnityEngine;

class CreateStable : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.STABLE; } }

    public CreateStable(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
