using UnityEngine;

public class GameOver : MonoBehaviour
{
    void Start() {}
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
        Application.LoadLevel(2);
    }
}
