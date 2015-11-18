using System;
using Storage;
using Managers;
using UnityEngine;

public abstract class CreateUnit : Ability
{
	private bool _enabled = false;
	private IGameEntity _entity;
	private GameObject civil;

    protected abstract UnitTypes type { get; }

	public CreateUnit(EntityAbility info, GameObject gameObject) :
		base(info, gameObject)
	{
		_entity = _gameObject.GetComponent<IGameEntity>();
	}

	public override bool isActive
	{
		get
		{
			return _enabled;
		}
	}
	/// <summary>
	/// Ability is not usable if player hasn't enough materials to spend in
	/// unit construction or build is under construction.
	// Best way to check if building is finished is to check if it has the
	// default unit, which is created just when building becomes usable.
	/// </summary>
	public override bool isUsable
	{
		get
		{

			UnitInfo unitInfo;
			unitInfo = Info.get.of(_entity.getRace(), type);

			//Debug.Log("*********   harvestUnits: " + res.harvestUnits);
			//Debug.Log("*********   Building status: " + res.status.ToString());
			//Debug.Log("*********   Create civilian status: " + res.status.ToString());

			return

				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, unitInfo.resources.food) &&
				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, unitInfo.resources.metal) &&
				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, unitInfo.resources.wood) &&
                //Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.GOLD, unitInfo.resources.gold) &&
                ((_entity.status == EntityStatus.IDLE)|| (_entity.status == EntityStatus.WORKING));
		}
	}


	public override void disable()
	{
		_enabled = false;
		base.disable();
	}

	public override void enable()
	{
		base.enable();

		if (_entity.info.isResource) {
			((Resource)_entity).addUnitQueue(type);
		} else if (_entity.info.isBarrack) {
			((Barrack)_entity).addUnitQueue(type);
		}
	}
}
