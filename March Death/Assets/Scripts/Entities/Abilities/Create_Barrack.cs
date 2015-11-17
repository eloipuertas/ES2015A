using System;
using Storage;

using UnityEngine;

class CreateBarrack : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.BARRACK; } }

    public CreateBarrack(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
