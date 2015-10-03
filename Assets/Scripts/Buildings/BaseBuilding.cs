using UnityEngine;
using System.Collections;

public class BaseBuilding: MonoBehaviour {
	protected string buildingClassName;
	protected string buildingClassDescription;
	protected string[] actions = {};

	protected int cost;
	protected int sellValue;
	protected int timeToBuild; // Seconds
	protected int hp;
	protected int maxHp;
	protected int repairSpeed; // hp/second

	protected Vector3 gridSize;
	protected Vector3 gridPosition; // Where it's located in the map

	protected bool underConstruction; 
	protected bool selected = false;
	protected bool highlight = false;
	public bool canAttack;

	// Should define which player owns the building
	



	protected virtual void Awake() {
		// set gridPosition  
	}
	
	protected virtual void Start () {

	}
	
	protected virtual void Update () {

	}
	
	protected virtual void OnGUI() {

	}


	//#### Public functions ####

	public void OnMouseUp() {
		if (!selected) {
			selected = true;
			Debug.Log (buildingClassName + " selected");
		}
	}

	public void OnMouseEnter() {
		highlight = true;
		Debug.Log (buildingClassName + " highlighted");
	}

	public void OnMouseExit() {
		highlight = false;
		Debug.Log (buildingClassName + " not highlighted");
	}

	public bool UnderConstruction() {
		return underConstruction;
	}

	public void StartConstruction() {
		underConstruction = true;
		hp = 0;
	}

	public void Constuct(int points) {
		hp += points;

		if (hp >= maxHp) {
			underConstruction = false;
		}
	}

	public float HealthPercentage() {
		return (hp * 100) / maxHp;
	}

	public string[] GetAction() {
		return actions;
	}

	public void ReceiveAttack(int points) {
		hp -= points;

		if (hp <= 0) {
			Destroy(gameObject);
		}
	}
}
