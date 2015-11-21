using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Storage;

/// <summary>
/// Script to handle UI events for the game setup screen.
/// </summary>
public class GameSetupScript : MonoBehaviour
{

    GameInformation info;

    /// <summary>
    /// Indicates whether the player has selected their civilization or not
    /// </summary>
    private bool raceSelected;

    /// <summary>
    /// Indicates if the message box should be shown
    /// </summary>
    private bool showMsgBox;

    /// <summary>
    /// The message box rectangle.
    /// </summary>
    private Rect messageBox = new Rect((Screen.width - 200) / 2, (Screen.height - 300) / 2, 200, 150);

    // Use this for initialization
    void Start ()
    {
        info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
        Debug.Log("info exists");
        raceSelected = false;
        showMsgBox = false;
    }

    void OnGUI()
    {
        if (showMsgBox)
        {
            messageBox = GUI.Window(0, messageBox, DrawWindow, "Select Race");
        }
    }

    /// <summary>
    /// Draws the message box.
    /// </summary>
    /// <param name="window">Window.</param>
    void DrawWindow(int window)
    {
        GUI.Label(new Rect(5, 20, messageBox.width, 20), "Please, select a civilization");
        if (GUI.Button(new Rect(5, 120, messageBox.width - 10, 20), "Ok"))
        {
            showMsgBox = false;
        }
    }

    public void SetPlayerRaceToElf ()
    {
        // HACK Even though info is initialized in Start(), it is null when it gets here
        if (!info) info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
        info.SetPlayerRace(Races.ELVES);
        raceSelected = true;
    }

    public void SetPlayerRaceToHuman()
    {
        // HACK Even though info is initialized in Start(), it is null when it gets here
        if (!info) info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
        info.SetPlayerRace(Races.MEN);
        raceSelected = true;
    }

    // TODO Implement mechanism to select game mode
    public void SetGameMode ()
    {
        info.setGameMode(GameInformation.GameMode.CAMPAIGN);
    }

    public void StartGame()
    {
        if (raceSelected)
        {
            GameObject menuMusic = GameObject.Find("BackgroundMusic");
            if (menuMusic)
            {
                Destroy(menuMusic);
            }
            SetGameMode();
            Application.LoadLevel(3);
        }
        else
        {
            showMsgBox = true;
        }
    }

    public void Cancel()
    {
        Application.LoadLevel(0);
    }
}
