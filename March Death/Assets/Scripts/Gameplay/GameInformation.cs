using UnityEngine;
using System.Collections;
using Storage;

public class GameInformation : MonoBehaviour {

    private Races playerRace;

    private static string pauseMenuPrefab;

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadHUD()
    {
        switch (playerRace)
        {
        case Races.ELVES:
            LoadElfHUD();;
            break;
        case Races.MEN:
            LoadHumanHUD();
            break;
        }
    }

    // TODO Modify to use the actual HUD for each civilization

    private static void LoadElfHUD()
    {
        Instantiate((GameObject)Resources.Load ("HUD_ELF2")).name = "HUD";
        Instantiate((GameObject)Resources.Load ("HUD_EventSystem")).name = "HUD_EventSystem";
    }

    private static void LoadHumanHUD()
    {
        Instantiate((GameObject)Resources.Load ("HUD-Human")).name = "HUD";
        //Application.LoadLevelAdditive("globalHUDHuman");
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
}
