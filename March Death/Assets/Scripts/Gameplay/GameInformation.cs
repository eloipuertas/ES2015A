using UnityEngine;
using System.Collections;
using Storage;

public class GameInformation : MonoBehaviour {

    private Races playerRace;
    private GameObject currentHud = null;

    public enum GameMode {CAMPAIGN, SKIRMISH};
    private GameMode gameMode;

    private Battle game;

    private static string pauseMenuPrefab;

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadHUD()
    {
        // TODO: reload different hud while playing
        //if (currentHud) Destroy(currentHud);
        switch (playerRace)
        {
        case Races.ELVES:
            LoadElfHUD();
            break;
        case Races.MEN:
            LoadHumanHUD();
            break;
        }
    }

    private void LoadElfHUD()
    {

        currentHud = Instantiate((GameObject)Resources.Load ("HUD-Elf"));
        currentHud.name = "HUD";
        Instantiate((GameObject)Resources.Load ("HUD_EventSystem")).name = "HUD_EventSystem";
    }

    private void LoadHumanHUD()
    {
        currentHud = Instantiate((GameObject)Resources.Load("HUD-Human"));
        currentHud.name = "HUD";
        Instantiate((GameObject)Resources.Load ("HUD_EventSystem")).name = "HUD_EventSystem";
    }

    public void SetPlayerRace(int race)
    {
        playerRace = (Races) race;
        switch(playerRace)
        {
            case Races.ELVES:
                pauseMenuPrefab = "PauseMenu-Elf";
                break;
            case Races.MEN:
                pauseMenuPrefab = "PauseMenu-Human";
                break;
        }
    }

    public void SetPlayerRace(Races race)
    {
        playerRace = race;
        switch(playerRace)
        {
            case Races.ELVES:
                pauseMenuPrefab = "PauseMenu-Elf";
                break;
            case Races.MEN:
                pauseMenuPrefab = "PauseMenu-Human";
                break;
        }
    }

    public Races GetPlayerRace()
    {
        return playerRace;
    }

    public string GetPauseMenuPrefabPath()
    {
        return pauseMenuPrefab;
    }

    public void setGameMode(GameMode mode)
    {
        gameMode = mode;
        hardcodedBattle();    // HACK Sets the campaign while the real functionality isn't implemented
    }

    public GameMode getGameMode()
    {
        return gameMode;
    }

    public void SetBattle(Battle battle)
    {
        game = battle;
    }

    public Battle GetBattle()
    {
        return game;
    }

    private void hardcodedBattle()
    {
        // TODO Read battle information from a JSON file or resulting object from a JSON deserializer
        game = new Battle();
        Battle.MissionDefinition.TargetType t = new Battle.MissionDefinition.TargetType();
        t.unit = UnitTypes.HERO;
        game.AddMission(Battle.MissionType.DESTROY, 1, EntityType.UNIT, t, 0, true, "");
        Battle.PlayerInformation player = new Battle.PlayerInformation(Races.MEN);
        player.AddBuilding(BuildingTypes.STRONGHOLD, 801.4f, 753.6f);
        player.AddUnit(UnitTypes.HERO, 801.4f, 785f);
        player.SetInitialResources(2000, 2000, 2000, 0);
        game.AddPlayerInformation(player);
        player = new Battle.PlayerInformation(Races.ELVES);
        player.AddUnit(UnitTypes.HERO, 726, 765);
        player.AddBuilding(BuildingTypes.STRONGHOLD, 706, 792);
        player.SetInitialResources(2000, 2000, 2000, 0);
        game.AddPlayerInformation(player);
        game.SetWorldResources(5000, 5000, 5000);
    }

    void OnDestroy()
    {
        Debug.Log("GameInformation destroyed");
    }
}
