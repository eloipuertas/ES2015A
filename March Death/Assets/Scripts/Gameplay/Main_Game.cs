using UnityEngine;
using Storage;

public class Main_Game : MonoBehaviour
{
    private GameInformation info;
    private Player user;
    Managers.BuildingsManager bm;
    Managers.SoundsManager sounds;
    public Managers.BuildingsManager BuildingsMgr { get { return bm; } }

    // Use this for initialization
    void Start()
    {
        if (GameObject.Find("GameInformationObject"))
            info = (GameInformation)GameObject.Find("GameInformationObject").GetComponent("GameInformation");
        bm = new Managers.BuildingsManager();
        sounds = GameObject.Find("GameController").GetComponent<Managers.SoundsManager>();
        if (info) info.LoadHUD();
        StartGame();
        bm.Player = user;
        UserInput inputs = gameObject.AddComponent<UserInput>();
        inputs.TerrainLayerMask = new LayerMask();
        inputs.TerrainLayerMask = 520;// HACK LayerMask.NameToLayer("Terrain"); didn't work
        bm.Inputs = inputs;
        LoadInitialScreen();
    }

    public GameInformation GetGameInformationObject()
    {
        return info;
    }

    void LoadInitialScreen()
    {
        GameObject welcomeScreen = null;
        switch (info.getGameMode())
        {
            case GameInformation.GameMode.SKIRMISH:
                switch (info.GetPlayerRace())
                {
                    case Races.ELVES:
                        welcomeScreen = (GameObject) Instantiate(Resources.Load("WelcomeScreen-Elf"));
                        break;
                    case Races.MEN:
                        welcomeScreen = (GameObject) Instantiate(Resources.Load("WelcomeScreen-Human"));
                        break;
                }
                break;
            case GameInformation.GameMode.CAMPAIGN:
                if (Application.loadedLevelName.Equals("ES2015A_Q1"))
                {
                    welcomeScreen = (GameObject) Instantiate(Resources.Load("mission1"));
                }
                else if (Application.loadedLevelName.Equals("ES2015A_Q2"))
                {
                    welcomeScreen = (GameObject) Instantiate(Resources.Load("mission2"));
                }
                else if (Application.loadedLevelName.Equals("ES2015A_Q3"))
                {
                    welcomeScreen = (GameObject) Instantiate(Resources.Load("mission3"));
                }
                else if (Application.loadedLevelName.Equals("ES2015A_Q4"))
                {
                    welcomeScreen = (GameObject) Instantiate(Resources.Load("mission4"));
                }
                break;
        }
        welcomeScreen.name = "Welcome-Screen";
    }

    public void StartGame()
    {
        switch (info.getGameMode())
        {
            case GameInformation.GameMode.CAMPAIGN:
                LoadCampaign();
                break;
            case GameInformation.GameMode.SKIRMISH:
                LoadSkirmish();
                break;
        }

        BasePlayer.Setup();
    }

    private void LoadCampaign()
    {
        info.SetStoryBattle();
        loadPlayers();
    }

    private void loadPlayers()
    {
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

    private void LoadSkirmish()
    {
        loadPlayers();
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
