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
}
