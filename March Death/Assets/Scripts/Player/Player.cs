using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

	//the list of player units in the scene
	public ArrayList currentUnits = new ArrayList ();

	public ArrayList SelectedObjects = new ArrayList();
    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update() { }

	public void FillPlayerUnits(GameObject unit) 
	{
		currentUnits.Add (unit);
	}
}
