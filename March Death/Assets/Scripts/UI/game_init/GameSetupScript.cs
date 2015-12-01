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
    /// Indicates whether the difficulty level has been selected or not.
    /// </summary>
    private bool difficultySelected;

    /// <summary>
    /// Indicates whether the game mode has been selected or not.
    /// </summary>
    private bool gameModeSelected;

    private enum ErrorType { CIVILIZATION_MISSING, SKILL_MISSING, GAME_MODE_MISSING }

    /// <summary>
    /// Holds the reference to the prefabs that have the error messages.
    /// </summary>
    private Object[] prefabs;

    // Use this for initialization
    void Start()
    {
        info = (GameInformation) GameObject.Find("GameInformationObject").GetComponent("GameInformation");
        raceSelected = false;
        difficultySelected = false;
        gameModeSelected = false;
        prefabs = new Object[3];
        prefabs[0] = Resources.Load("Prefabs/ErrorMessages/SelectCivilizationMessageError");
        prefabs[1] = Resources.Load("Prefabs/ErrorMessages/ChooseGameModeMessageError");
        prefabs[2] = Resources.Load("Prefabs/ErrorMessages/SkillLevelMessageError");
    }

    public void SetPlayerRaceToElf()
    {
        info.SetPlayerRace(Races.ELVES);
        raceSelected = true;
    }

    public void SetPlayerRaceToHuman()
    {
        info.SetPlayerRace(Races.MEN);
        raceSelected = true;
    }

    public void SetDifficultyLevel(int level)
    {
        info.Difficulty = level;
        difficultySelected = true;
    }

    public void SetGameMode(GameInformation.GameMode mode)
    {
        info.setGameMode(mode);
        gameModeSelected = true;
    }

    private void showErrorMessage(ErrorType error)
    {
        GameObject msgBox = null; // Set to null to avoid errors on compilation time
        switch (error)
        {
            case ErrorType.CIVILIZATION_MISSING:
                msgBox = (GameObject) Instantiate(prefabs[0]);
                break;
            case ErrorType.GAME_MODE_MISSING:
                msgBox = (GameObject) Instantiate(prefabs[1]);
                break;
            case ErrorType.SKILL_MISSING:
                msgBox = (GameObject) Instantiate(prefabs[2]);
                break;
        }
        msgBox.name = "ErrorDialog";
        msgBox.GetComponent<Button>().onClick.AddListener(() => {
            GameObject obj = GameObject.Find("ErrorDialog");
            Destroy(obj);
        });
    }

    public void StartGame()
    {
        if (raceSelected && gameModeSelected && difficultySelected)
        {
            GameObject menuMusic = GameObject.Find("BackgroundMusic");
            if (menuMusic)
            {
                Destroy(menuMusic);
            }
            Application.LoadLevel(3);
        }
        else
        {
            if (!raceSelected)
                showErrorMessage(ErrorType.CIVILIZATION_MISSING);
            if (!difficultySelected)
                showErrorMessage(ErrorType.SKILL_MISSING);
            if (!gameModeSelected)
                showErrorMessage(ErrorType.GAME_MODE_MISSING);
        }
    }

    public void Cancel()
    {
        Application.LoadLevel(0);
    }
}
