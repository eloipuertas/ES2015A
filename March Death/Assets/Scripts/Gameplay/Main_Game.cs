using UnityEngine;
using System.Collections;
using Storage;

public class Main_Game : MonoBehaviour {

	private GameInformation info;
	Transform strongholdTransform;

	public GameObject playerStronghold;	// TODO Move to Player class

	// Use this for initialization
	void Start () {
		strongholdTransform = GameObject.Find("PlayerStronghold").transform;
		info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
		LoadPlayerStronghold();
		Player.print("The race: " + info.GetPlayerRace());
		info.LoadHUD();
    }

	private void LoadPlayerStronghold()
	{
		switch(info.GetPlayerRace())
		{
		case Races.MEN:
			InstantiateHumanStronghold();
			break;
		case Races.ELVES:
			InstantiateElfStronghold();
			break;
		}
	}

	// TODO Add stronghold reference to player

	void InstantiateElfStronghold()
	{
        GameObject stronghold;
        stronghold = (GameObject) Resources.Load("Prefabs/Buildings/Elf-Stronghold", typeof(GameObject));
		playerStronghold = (GameObject) Instantiate(stronghold, strongholdTransform.position, 
		                         strongholdTransform.rotation);
	}

	void InstantiateHumanStronghold()
	{
		GameObject stronghold;
		stronghold = (GameObject) Resources.Load("Prefabs/Buildings/Human-Stronghold", typeof(GameObject));
		playerStronghold = (GameObject) Instantiate(stronghold, strongholdTransform.position, 
		            strongholdTransform.rotation);
	}
}
