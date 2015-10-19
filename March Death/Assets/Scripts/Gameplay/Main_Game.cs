using UnityEngine;
using System.Collections;
using Storage;

public class Main_Game : MonoBehaviour {

	private GameInformation info;
	Transform strongholdTransform;

	public GameObject playerStronghold;
    public GameObject playerHero;

	// Use this for initialization
	void Start () {
		strongholdTransform = GameObject.Find("PlayerStronghold").transform;
        playerHero = GameObject.Find("PlayerHero");
		info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
		LoadPlayerStronghold();
        LoadPlayerUnits();
		info.LoadHUD();
    }

	private void LoadPlayerStronghold()
	{
        // TODO Add stronghold reference to the player
        playerStronghold = Info.get.createBuilding(info.GetPlayerRace(), 
                                                   BuildingTypes.STRONGHOLD, 
                                                   strongholdTransform.position, strongholdTransform.rotation);
	}

    private void LoadPlayerUnits()
    {
        // TODO Must be able to load other kinds of units (both civilian and military)
        playerHero = Info.get.createUnit(info.GetPlayerRace(), 
                                         UnitTypes.HERO, playerHero.transform.position, 
                                         playerHero.transform.rotation);
    }
}
