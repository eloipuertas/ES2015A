using UnityEngine;
using System.Collections;

public class BuildingsFactory:  MonoBehaviour{

    Player player;
    UserInput inputs;

    private bool _locating;
    public bool Locating {
        get
        {
            return _locating;
        }
    }

    private GameObject newBuilding;

	// Use this for initialization
	void Start ()
    {
        player = GameObject.Find("GameObject").GetComponent<Player>();
        inputs = player.GetComponent<UserInput>();
        _locating = false;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (_locating)
        {
            relocate();

        }
	
	}

    private void relocate()
    {

        Vector3 toLocation = inputs.FindHitPoint();
        toLocation = adaptLocation(toLocation);

        // alter the color if is not a valid location
        if (checkLocation(toLocation))
        {
            newBuilding.GetComponent<Utils.ObjectColors>().alterColor(Utils.ObjectColors.colors.GREEN);
        }
        else
        {
            newBuilding.GetComponent<Utils.ObjectColors>().alterColor(Utils.ObjectColors.colors.RED);
        }

            newBuilding.transform.position = toLocation;
            Cursor.visible = false;
    }

    /// <summary>
    /// Starts creating a building, required the name of the building ex: 'elf-farm'
    /// </summary>
    /// <param name="name"></param>
    public void createBuilding(string name)
    {
        if (!_locating)
        {
            newBuilding = (GameObject)Resources.Load("Prefabs/Buildings/" + name, typeof(GameObject));
            newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
            newBuilding.AddComponent<Utils.ObjectColors>();
            _locating = true;
        }


    }


    /// <summary>
    /// Discretizes the location through ConstructioGrid
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private Vector3 adaptLocation(Vector3 location)
    {
        //location = Utils.ConstructionGrid.isValidLocationc(toLocation);
        return location;
    }

    /// <summary>
    /// Checks if is valid locatoin through ConstructorGrid
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private bool checkLocation( Vector3 location)
    {
        bool check;

        
        /* Utils.ConstructionGrid.isValidLocation(toLocation)*/
        check = location.y < 90 ? true: false;

        return check;
    
    }

    /// <summary>
    /// Places the building, checking if is a suitable place
    /// </summary>
    public void placeBuilding()
    {
        Vector3 toLocation = inputs.FindHitPoint();
        toLocation = adaptLocation(toLocation);

        // alter the color if is not a valid location
        if (checkLocation(toLocation))
        {
            //Utils.ConstructionGrid.resevePosition(toLocation)
            newBuilding.transform.position = toLocation;
            Cursor.visible = true;
            _locating = false;
            IGameEntity destination = (IGameEntity)newBuilding.GetComponent<Unit>();
            player.addEntityToList(destination);
        }
        

    }

}
