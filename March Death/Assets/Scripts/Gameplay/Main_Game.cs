using UnityEngine;
using System.Collections;
using Storage;

public class Main_Game : MonoBehaviour
{

    private GameInformation info;
    private CameraController cam;
    private Player user;
    Managers.BuildingsManager bm;

    Transform strongholdTransform;
    GameObject playerHero;

    // Use this for initialization
    void Start ()
    {
        strongholdTransform = GameObject.Find ("PlayerStronghold").transform;
        playerHero = GameObject.Find ("PlayerHero");
        if (GameObject.Find ("GameInformationObject"))
            info = (GameInformation)GameObject.Find ("GameInformationObject").GetComponent ("GameInformation");
        user = GameObject.Find ("GameController").GetComponent ("Player") as Player;
        cam = GameObject.FindWithTag ("MainCamera").GetComponent<CameraController> ();
        bm = GameObject.Find ("GameController").GetComponent<Managers.BuildingsManager> ();
        if (info)
            info.LoadHUD ();
        LoadPlayerStronghold ();
        LoadPlayerUnits ();
    }

    private void LoadPlayerStronghold ()
    {
        GameObject playerStronghold;
        if (info)
        {
            /*playerStronghold = Info.get.createBuilding(info.GetPlayerRace(),
                                                       BuildingTypes.STRONGHOLD,
                                                   strongholdTransform.position, strongholdTransform.rotation);*/
            playerStronghold = bm.createBuilding (strongholdTransform.position, 
                              strongholdTransform.rotation, 
                              BuildingTypes.STRONGHOLD, info.GetPlayerRace ());
            user.addEntity (playerStronghold.GetComponent<IGameEntity> ());
            cam.lookGameObject (playerStronghold);
        }
    }

    private void LoadPlayerUnits ()
    {
        if (info)
        {
            // TODO Must be able to load other kinds of units (both civilian and military)
            playerHero = Info.get.createUnit (info.GetPlayerRace (),
                                             UnitTypes.HERO, playerHero.transform.position,
                                         playerHero.transform.rotation);
            //user.FillPlayerUnits(playerHero);
            user.addEntity (playerHero.GetComponent<IGameEntity> ());
        }
    }

    public GameInformation GetGameInformationObject ()
    {
        return info;
    }

    public void StartGame ()
    {
        switch (info.getGameMode ()) {
        case GameInformation.GameMode.CAMPAIGN:
            LoadCampaign ();
            break;
        case GameInformation.GameMode.SKIRMISH:
            break;
        }
    }

    private void LoadCampaign ()
    {
        foreach (Battle.PlayerInformation player in info.GetBattle().GetPlayerInformationList())
        {
            foreach (Battle.PlayableEntity building in player.GetBuildings())
            {
                // TODO BuildingsManager must create building in the given grid position
                // IMPORTANT: Take into account all 3 axis positions
            }
            foreach (Battle.PlayableEntity unit in player.GetUnits())
            {
                // TODO Create the units in the given grid position
                // IMPORTANT: Take into account all 3 axis positions
            }
            // TODO Set player's initial resources
        }
        // TODO Set initial resources in the map
    }

    public void ClearGame ()
    {
        GameObject obj;
        // Unregisters events in the HUD
        obj = GameObject.Find ("HUD");
        obj.GetComponentInChildren<InformationController> ().Clear ();
        obj.GetComponentInChildren<EntityAbilitiesController> ().Clear ();
        obj = GameObject.Find ("GameInformationObject").gameObject;
        Destroy (obj);
    }
}
