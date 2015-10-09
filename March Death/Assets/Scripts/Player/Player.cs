using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    private List<IGameEntity> _activeEntities = new List<IGameEntity>();
    public bool locatingBuilding { get; set; }
    public Selectable SelectedObject { get; set; }
    
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

    /// <summary>
    /// Returns the count of the current associated entities
    /// </summary>
    /// <returns></returns>
    public int currentEntitiesCount()
    {
        return _activeEntities.Count;
    }
}
