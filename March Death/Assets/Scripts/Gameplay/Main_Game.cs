using UnityEngine;
using System.Collections.Generic;
using Storage;

public class Main_Game : MonoBehaviour
{

    private GameInformation info;
    private Player user;
    Managers.BuildingsManager bm;
    Managers.SoundsManager sounds;
    public Managers.BuildingsManager BuildingsMgr { get { return bm; } }

    // Use this for initialization
    void Start ()
    {
        //strongholdTransform = GameObject.Find ("PlayerStronghold").transform;
        //playerHero = GameObject.Find ("PlayerHero");
        if (GameObject.Find ("GameInformationObject"))
            info = (GameInformation)GameObject.Find ("GameInformationObject").GetComponent ("GameInformation");
        //user = GameObject.Find ("GameController").GetComponent ("Player") as Player;
        //bm = GameObject.Find ("GameController").GetComponent<Managers.BuildingsManager> ();
        bm = new Managers.BuildingsManager();
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

    public GameInformation GetGameInformationObject()
    {
        return info;
    }

    public void StartGame()
    {
        switch (info.getGameMode())
        {
            case GameInformation.GameMode.CAMPAIGN:
                LoadCampaign();
                break;
            case GameInformation.GameMode.SKIRMISH:
                break;
        }

        BasePlayer.Setup();
    }

    private void LoadCampaign ()
    {
        GameObject created;
        Vector3 position;
        int id = 1;
        foreach (Battle.PlayerInformation player in info.GetBattle().GetPlayerInformationList())
        {
            // TODO It would be better if it wasn't race dependent
            if (player.Race != info.GetPlayerRace())
            {
                Assets.Scripts.AI.AIController ai = gameObject.AddComponent<Assets.Scripts.AI.AIController>();
                ai.PlayerID = id;
            }
            else
            {
                user = gameObject.AddComponent<Player>();
                user.PlayerID = id;
            }
            id++;
        }
        // TODO Set initial resources in the map
    }

    public void ClearGame()
    {
        GameObject obj;
        obj = GameObject.Find("GameInformationObject").gameObject;
        Destroy(obj);
    }

    void Update()
    {
        bm.Update();
    }
}
