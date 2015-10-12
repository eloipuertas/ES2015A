using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Information regarding the current status of the player
    /// </summary>
    private status _currently;
    public enum status {IDLE, PLACING_BUILDING, SELECTING_UNITS /*...*/}

    /// <summary>
    /// Information regarding the entities of the player
    /// </summary>
    private List<IGameEntity> _activeEntities = new List<IGameEntity>();

    
	//the list of player units in the scene
	public ArrayList currentUnits = new ArrayList ();

	public ArrayList SelectedObjects = new ArrayList();
    // Use this for initialization
    void Start(){}

    // Update is called once per frame
    void Update() { }
    /// <summary>
    /// Add a IGameEntity to the list
    /// Player has a list with all the entities associated to him
    /// </summary>
    /// <param name="newEntity"></param>
    public void addEntityToList(IGameEntity newEntity)
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
}
