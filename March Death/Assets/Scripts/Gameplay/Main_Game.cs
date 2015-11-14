using UnityEngine;
using System.Collections.Generic;
using Storage;

public class Main_Game : MonoBehaviour
{

    private GameInformation info;
    private CameraController cam;
    private Player user;
    Managers.BuildingsManager bm;
    Managers.SoundsManager sounds;
    public Managers.BuildingsManager BuildingsMgr { get { return bm; } }

    private List<BasePlayer> allPlayers;

    Terrain terrain;
    GameObject gameController;

    // Use this for initialization
    void Start ()
    {
        allPlayers = new List<BasePlayer>(2);
        //strongholdTransform = GameObject.Find ("PlayerStronghold").transform;
        //playerHero = GameObject.Find ("PlayerHero");
        if (GameObject.Find ("GameInformationObject"))
            info = (GameInformation)GameObject.Find ("GameInformationObject").GetComponent ("GameInformation");
        //user = GameObject.Find ("GameController").GetComponent ("Player") as Player;
        cam = GameObject.FindWithTag ("MainCamera").GetComponent<CameraController> ();
        //bm = GameObject.Find ("GameController").GetComponent<Managers.BuildingsManager> ();
        bm = new Managers.BuildingsManager();
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        sounds = GameObject.Find("GameController").GetComponent<Managers.SoundsManager>();
        if (info) info.LoadHUD ();
        //LoadPlayerStronghold ();
        //LoadPlayerUnits ();
        StartGame();
        bm.Player = user;
        UserInput inputs = gameObject.AddComponent<UserInput>();
        inputs.TerrainLayerMask = new LayerMask();
        inputs.TerrainLayerMask = 520;// HACK LayerMask.NameToLayer("Terrain"); didn't work
        bm.Inputs = inputs; 
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
        GameObject created;
        Vector3 position;
        foreach (Battle.PlayerInformation player in info.GetBattle().GetPlayerInformationList())
        {
            BasePlayer basePlayer;
            // TODO It would be better if it wasn't race dependent
            if (player.Race != info.GetPlayerRace())
            {
                basePlayer = gameObject.AddComponent<Assets.Scripts.AI.AIController>();
            }
            else
            {
                basePlayer = gameObject.AddComponent<Player>();
                user = (Player) basePlayer;
            }
            basePlayer.Start();
            foreach (Battle.PlayableEntity building in player.GetBuildings())
            {
                position = new Vector3();
                position.x = building.position.X;
                position.z = building.position.Y;
                // HACK Without the addition, Construction Grid detects the terrain as it not being flat
                position.y = 1 + terrain.SampleHeight(position);
                created = bm.createBuilding(position, Quaternion.Euler(0,0,0),
                                            building.entityType.building,
                                            player.Race);
                basePlayer.addEntity(created.GetComponent<IGameEntity>());
                if (building.entityType.building == BuildingTypes.STRONGHOLD &&
                    info.GetPlayerRace() == basePlayer.race)
                {
                    cam.lookGameObject(created);
                }
            }
            foreach (Battle.PlayableEntity unit in player.GetUnits())
            {
                position = new Vector3();
                position.x = unit.position.X;
                position.z = unit.position.Y;
                position.y = terrain.SampleHeight(position);
                created = Info.get.createUnit(player.Race, unit.entityType.unit,
                                              position, Quaternion.Euler(0,0,0));
                basePlayer.addEntity(created.GetComponent<IGameEntity>());
            }
            basePlayer.SetInitialResources(player.GetResources().Wood,
                                           player.GetResources().Food,
                                           player.GetResources().Metal);
            allPlayers.Add(basePlayer);
        }
        // TODO Set initial resources in the map
    }

    public void ClearGame()
    {
        GameObject obj;
        // Unregisters events in the HUD
        obj = GameObject.Find("HUD");
        obj.GetComponentInChildren<InformationController>().Clear();
        obj.GetComponentInChildren<EntityAbilitiesController>().Clear();
        obj = GameObject.Find ("GameInformationObject").gameObject;
        Destroy(obj);
    }

    void Update()
    {
        bm.Update();
    }
}
