using UnityEngine;
using System.Collections;

public class EntitySelection : MonoBehaviour {

    private Projector selection;
    private Color _selectionColor;


    void Awake()
    {
        selection = GetComponent<Projector>();
    }
	// Use this for initialization
	void Start ()
    {
        selection.enabled = false;
	}


    /// <summary>
    /// Sets the color for the current race
    /// </summary>
    /// <param name="race"></param>
    public void SetColorRace(Storage.Races race)
    {
        switch (race)
        {
            case Storage.Races.ELVES:
                _selectionColor = Color.green;
                break;
            case Storage.Races.MEN:
                _selectionColor = Color.red;
                break;
        }

        selection.material.color = _selectionColor;
        
    }
    
    public void Enable()
    {
        selection.enabled = true;
    }


    public void Disable()
    {
        selection.enabled = false;
    }
}
