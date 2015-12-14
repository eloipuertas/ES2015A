using UnityEngine;
using System.Collections;

public class MissionScreen : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 0;
    }

    public void Close()
    {
        Time.timeScale = 1;
        Destroy(GameObject.Find("Welcome-Screen"));
    }

    public void LoadNextLevel()
    {
        Application.LoadLevel(Application.loadedLevel + 1);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        Application.LoadLevel(0);
    }

    public void RestartMission() {}
}

