using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : BasePlayer
{
    /// <summary>
    /// Information regarding the current status of the player
    /// </summary>
    private status _currently;
    public status currently { get { return _currently; }}
    public enum status {IDLE, PLACING_BUILDING, SELECTED_UNITS, TERMINATED /*...*/}

    /// <summary>
    /// Information regarding the entities of the player
    /// </summary>
    private List<IGameEntity> _activeEntities = new List<IGameEntity>();
    public List<IGameEntity> activeEntities {
        get { return new List<IGameEntity>(_activeEntities); }
    }


    //the list of player units in the scene
    public ArrayList currentUnits = new ArrayList ();

    // i order to mantain InformationController working
	//public ArrayList SelectedObjects = new ArrayList();
    public ArrayList SelectedObjects { get { return _selection.ToArrayList(); } }

    private bool showMsgBox = false;
    private Rect messageBox = new Rect((Screen.width - 200) / 2, (Screen.height - 300) / 2, 200, 150);
    private string strStatus = "";

    // Use this for initialization
    public override void Start()
    {   
        base.Start();
        _buildings = GetComponent<Managers.BuildingsManager>();
        _selection = GetComponent<Managers.SelectionManager>();
        //request the race of the player
        _selfRace = info.GetPlayerRace();
        _selection.SetRace(race);
        
        AcquirePlayerID();
        missionStatus = new MissionStatus(playerId);
    }

    // Update is called once per frame
    void Update()
    {
        if (missionStatus.isGameOver())
        {
            if (strStatus.Equals(""))
            {
                GameObject gameOverDialog = null;
                if (missionStatus.hasWon(playerId))
                {
                    strStatus = "You win!";
                    switch (_selfRace)
                    {
                        case Storage.Races.MEN:
                        case Storage.Races.ELVES:
                            gameOverDialog = (GameObject) Resources.Load("GameEndWinElf");
                            break;
                    }
                }
                else
                {
                    strStatus = "You loose";
                    switch (_selfRace)
                    {
                        case Storage.Races.MEN:
                        case Storage.Races.ELVES:
                            gameOverDialog = (GameObject) Resources.Load("GameOver-Elf");
                            break;
                    }
                }
                Instantiate(gameOverDialog);
            }
            _currently = status.TERMINATED;
        }
    }

    void OnGUI()
    {
        if (showMsgBox)
        {
            messageBox = GUI.Window(0, messageBox, DrawWindow, "Game Over!");
        }
    }
    
    /// <summary>
    /// Draws the message box.
    /// </summary>
    /// <param name="window">Window.</param>
    void DrawWindow(int window)
    {
        GUI.Label(new Rect(5, 20, messageBox.width, 20), strStatus);
        if (GUI.Button(new Rect(5, 120, messageBox.width - 10, 20), "Ok"))
        {
            showMsgBox = false;
            Application.LoadLevel(0);
        }
    }

    public override void removeEntity(IGameEntity entity)
    {
        _activeEntities.Remove(entity);
    }

    /// <summary>
    /// Add a IGameEntity to the list
    /// Player has a list with all the entities associated to him
    /// </summary>
    /// <param name="newEntity"></param>
    public override void addEntity(IGameEntity newEntity)
    {
        _activeEntities.Add(newEntity);
        Debug.Log(_activeEntities.Count + " entities");
    }

	public void FillPlayerUnits(GameObject unit) 
	{
		currentUnits.Add (unit);
	}
    
    /// <summary>
    /// Returns the count of the current associated entities
    /// </summary>
    /// <returns></returns>
    public int currentEntitiesCount()
    {
        return _activeEntities.Count;
    }

    /// <summary>
    /// Returns whether is in the current status or not
    /// </summary>
    /// <param name="check"></param>
    /// <returns></returns>
    public bool isCurrently(status check)
    {
        return _currently == check;

    }

    /// <summary>
    /// Establishes the new status of the player
    /// </summary>
    /// <param name="newStatus"></param>
    public void setCurrently(status newStatus)
    {
        _currently = newStatus;
    }

    public ArrayList getSelectedObjects()
    {
        return (ArrayList) SelectedObjects.Clone();
    }

    // <summary>
    // Getter for the resources of the player.
    // </summary>
    //public Managers.ResourcesManager resources {get { return _resources; } }

    private void performUnitDied(System.Object obj)
    {
        IGameEntity e = ((GameObject) obj).GetComponent<IGameEntity>();
        missionStatus.OnUnitKilled(((Unit) e).type);
    }

    private void performBuildingDestroyed(System.Object obj)
    {
        IGameEntity e = ((GameObject) obj).GetComponent<IGameEntity>();
        if (e.info.isBarrack)
        {
            missionStatus.OnBuildingDestroyed(((Barrack) e).type);
        }
        else if (e.info.isResource)
        {
            missionStatus.OnBuildingDestroyed(((Resource) e).type);
        }
    }

    public void registerGameEntityActions(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            entity.registerFatalWounds(performUnitDied);
        }
        else if (entity.info.isBuilding)
        {
            entity.registerFatalWounds(performBuildingDestroyed);
        }
    }
}
