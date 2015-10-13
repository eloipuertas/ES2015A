using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Script to handle UI events for the game setup screen.
/// </summary>
public class GameSetupScript : MonoBehaviour {

    /// <summary>
    /// Constant value to identify the combo box for the player's race selection.
    /// </summary>
    const int PLAYER_RACE = 0;
    /*const int ENEMY_1_RACE = 1;*/

    private Dropdown playerComboBox;

	GameInformation info;

    // Use this for initialization
    void Start () {
        GameObject myComboBox = GameObject.Find("cboCivilizations");
        playerComboBox = myComboBox.GetComponent<Dropdown>();
		info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
    }

    // Update is called once per frame
    void Update () {}

    /// <summary>
    /// Sets the race of the players (both human and AI)
    /// </summary>
    /// <param name="combo">Combo box that triggers the event</param>
	public void setRace(int combo)
    {
        switch (combo)
        {
        case PLAYER_RACE:
			info.SetPlayerRace(playerComboBox.value);
            break;
        }
    }

    public void StartGame()
    {
		Application.LoadLevel(3);
    }

    public void Cancel()
    {
		Destroy(info);
		Application.LoadLevel(0);
    }
}
