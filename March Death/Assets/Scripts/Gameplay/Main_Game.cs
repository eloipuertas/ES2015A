using UnityEngine;
using System.Collections;
using Storage;

public class Main_Game : MonoBehaviour {

	private GameInformation info;
	Transform strongholdTransform;

	public GameObject playerStronghold;

	// Use this for initialization
	void Start () {
		strongholdTransform = GameObject.Find("PlayerStronghold").transform;
		info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
		LoadPlayerStronghold();
		info.LoadHUD();
    }

	private void LoadPlayerStronghold()
	{
        // TODO Add stronghold reference to player
        playerStronghold = Info.get.createBuilding(info.GetPlayerRace(), 
                                                   BuildingTypes.STRONGHOLD, 
                                                   strongholdTransform.position, strongholdTransform.rotation);
	}
}
