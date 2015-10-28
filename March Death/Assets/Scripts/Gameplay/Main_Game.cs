using UnityEngine;
using System.Collections;
using Storage;

public class Main_Game : MonoBehaviour {

	private GameInformation info;
	private CameraController cam;
	private Player user;

	Transform strongholdTransform;
	GameObject playerHero;
    ConstructionGrid grid;
    GameObject gameController;

    private Player player;

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindGameObjectWithTag("GameController");
        player = gameController.GetComponent<Player>();

        strongholdTransform = GameObject.Find("PlayerStronghold").transform;
        playerHero = GameObject.Find("PlayerHero");
        if(GameObject.Find("GameInformationObject"))
		    info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
		user = GameObject.Find("GameController").GetComponent("Player") as Player;
		cam = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
		if (cam == null) Debug.Log("WARNING: No CAM");
        if(info) info.LoadHUD();
        LoadPlayerStronghold();
        LoadPlayerUnits();
        grid = gameController.GetComponent<ConstructionGrid>();
	}

	private void LoadPlayerStronghold()
	{
		GameObject playerStronghold;
        if (info)
        {
            playerStronghold = Info.get.createBuilding(info.GetPlayerRace(),
                BuildingTypes.STRONGHOLD, strongholdTransform.position, strongholdTransform.rotation);

            // adding the building to the construction grid
            if(!grid) grid = gameController.GetComponent<ConstructionGrid>();
            Vector3 position = grid.discretizeMapCoords(strongholdTransform.position);
            grid.reservePosition(position);

			user.addEntity(playerStronghold.GetComponent<IGameEntity>());
			cam.lookGameObject(playerStronghold);
        }
	}

    private void LoadPlayerUnits()
    {
        if (info)
        {
            // TODO Must be able to load other kinds of units (both civilian and military)
            playerHero = Info.get.createUnit(info.GetPlayerRace(),
                UnitTypes.HERO, playerHero.transform.position, playerHero.transform.rotation);

            user.addEntity(playerHero.GetComponent<IGameEntity>());
        }
    }

    public GameInformation GetGameInformationObject()
    {
		return info;
    }

    public void ClearGame()
    {
        GameObject obj;
        // Unregisters events in the HUD
        obj = GameObject.Find("HUD");
        obj.GetComponentInChildren<InformationController>().Clear();
        obj.GetComponentInChildren<EntityAbilitiesController>().Clear();
        obj = GameObject.Find("GameInformationObject").gameObject;
        Destroy(obj);
    }
}
