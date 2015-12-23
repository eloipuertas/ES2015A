using UnityEngine;
using System.Collections;

public class MissionScreen : MonoBehaviour
{
    public const string SCREEN_NAME = "Welcome-Screen";

    void Start()
    {
        GameInformation info;
        Time.timeScale = 0;
        // TODO Find "estandarte"; if exists, then update appropriately
        info = GameObject.Find("GameInformationObject").GetComponent<GameInformation>();
        if (info.getGameMode() == GameInformation.GameMode.CAMPAIGN)
        {
            switch (info.GetPlayerRace())
            {
                case Storage.Races.ELVES:
                    GameObject.Find(SCREEN_NAME + "/estandarte2_Human").SetActive(false);
                    GameObject.Find(SCREEN_NAME + "/estandarte1_Human").SetActive(false);
                    break;
                case Storage.Races.MEN:
                    GameObject.Find(SCREEN_NAME + "/estandarte2_Elf").SetActive(false);
                    GameObject.Find(SCREEN_NAME + "/estandarte1_Elf").SetActive(false);
                    break;
            }
        }
    }

    public void Close()
    {
        Time.timeScale = 1;
        Destroy(GameObject.Find(SCREEN_NAME));
    }

    public void LoadNextLevel()
    {
        Application.LoadLevel(Application.loadedLevel + 1);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        GameObject main = GameObject.FindWithTag("GameController");
        if (main)
        {
            main.GetComponent<Main_Game>().ClearGame();
        }
        Application.LoadLevel(0);
    }

    /// <summary>
    /// Re-loads the current scene.
    /// </summary>
    public void RestartMission()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}

