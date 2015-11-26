using UnityEngine;

public class GameOver : MonoBehaviour
{
    Main_Game mg;

    void Start()
    {
        mg = GameObject.FindWithTag("GameController").GetComponent<Main_Game>();
    }

    void Update() {}

    void loadLevel(int level)
    {
        mg.ClearGame();
        Application.LoadLevel(level);
    }

    /// <summary>
    /// Loads the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        loadLevel(0);
    }

    /// <summary>
    /// Loads the civilization selection menu.
    /// </summary>
    public void RestartGame()
    {
        loadLevel(2);
    }
}
