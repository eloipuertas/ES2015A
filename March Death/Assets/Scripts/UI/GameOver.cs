using UnityEngine;

public class GameOver : MonoBehaviour
{
    Main_Game mg;

    void Start()
    {
        mg = GameObject.FindWithTag("GameController").GetComponent<Main_Game>();
    }

    void Update() {}

    /// <summary>
    /// Loads the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Application.LoadLevel(0);
    }

    /// <summary>
    /// Loads the civilization selection menu.
    /// </summary>
    public void RestartGame()
    {
        mg.ClearGame();
        Application.LoadLevel(2);
    }
}
