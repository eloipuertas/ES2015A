using UnityEngine;


public class Unit : MonoBehaviour {

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Storage.Races race = Storage.Races.MEN;
    public Storage.Types type = Storage.Types.HERO;

    private Storage.UnitInfo info;

	// Use this for initialization
	void Start () {
        info = Storage.InfoGather.get.getInfoOf(race, type);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
