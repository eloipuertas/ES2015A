using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameSetupScript : MonoBehaviour {

    /// <summary>
    /// Constant value to identify the combo box for the player's race selection.
    /// </summary>
    const int PLAYER_RACE = 0;
    /*const int ENEMY_1_RACE = 1;*/

    private int playerRace;

    //private Dropdown playerComboBox;

    // Use this for initialization
    void Start () {
        GameObject myComboBox = GameObject.Find("cboCivilizations");
        playerRace = 0;
        //playerComboBox = myComboBox.GetComponent<Dropdown>();
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
            ///playerRace = playerComboBox.value;
            break;
        }
    }

    public void StartGame()
    {
        // TODO Add logic to open hud with current setup (race, etc.)
    }

    public void Cancel()
    {
        // TODO Add logic to return to main menu
    }
}
