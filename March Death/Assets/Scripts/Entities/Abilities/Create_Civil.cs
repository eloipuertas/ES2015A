using System;
using Storage;
using Managers;
using UnityEngine;

class CreateCivil : Ability
{
	private bool _enabled = false;
	private IGameEntity _entity;
	private GameObject civil;
	
	public CreateCivil(EntityAbility info, GameObject gameObject) :
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
			unitInfo = Info.get.of(_entity.getRace(), UnitTypes.CIVIL);
			
			//Debug.Log("*********   harvestUnits: " + res.harvestUnits);
			//Debug.Log("*********   Building status: " + res.status.ToString());
			//Debug.Log("*********   Create civilian status: " + res.status.ToString());
			
			return 
				
				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.FOOD, unitInfo.resources.food) &&
				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.METAL, unitInfo.resources.metal) &&
				Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.WOOD, unitInfo.resources.wood) &&
				_entity.status == EntityStatus.IDLE;
			//Player.getOwner(_entity).resources.IsEnough(WorldResources.Type.GOLD, unitInfo.resources.gold) &&  

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
		
		IGameEntity entity;
		
		if (_entity.info.isResource) {
			((Resource)_entity).addUnitQueue (UnitTypes.CIVIL);
		} else if (_entity.info.isBarrack) {
			((Barrack)_entity).addUnitQueue (UnitTypes.CIVIL);
		}
		
		UnitInfo unitInfo = Info.get.of(_entity.getRace(), UnitTypes.CIVIL);
		Player.getOwner(_entity).resources.SubstractAmount(WorldResources.Type.WOOD, unitInfo.resources.wood);
		Player.getOwner(_entity).resources.SubstractAmount(WorldResources.Type.METAL, unitInfo.resources.metal);
		Player.getOwner(_entity).resources.SubstractAmount(WorldResources.Type.FOOD, unitInfo.resources.food);
	}
}
