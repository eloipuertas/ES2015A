using UnityEngine;
using System.Collections;

public class Fortress : BaseBuilding {


	protected override void Awake() {
		base.Awake();
	}
	
	protected override void Start () {
		base.Start();
		
		buildingClassName = "Fortress";
		buildingClassDescription = "The most important building";
		cost = 1000;
		timeToBuild = 120; // seconds
		hp = 100;
		repairSpeed = 1; // HP/second
		gridSize = new Vector3(10, 10, 0);
		actions = new string[] {"Upgrade"};
		canAttack = false;
	}
	
	protected override void Update () {
		base.Update();
	}
	
	protected override void OnGUI() {
		base.OnGUI();
	}
}