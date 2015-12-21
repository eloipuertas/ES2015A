using UnityEngine;
using System.Collections;

public class MissionScreen : MonoBehaviour
{
    public const string SCREEN_NAME = "Welcome-Screen";

    void Start()
    {
        Time.timeScale = 0;
        // TODO Find "estandarte"; if exists, then update appropriately
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

    public void RestartMission() {}
}

