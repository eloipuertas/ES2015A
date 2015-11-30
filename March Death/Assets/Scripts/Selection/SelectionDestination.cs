using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectionDestination : MonoBehaviour {

    private List<Selectable>_members;
    private Unit.Actions[] _actions = { Unit.Actions.MOVEMENT_END, Unit.Actions.DIED };

    void Awake()
    {
        gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start (){}

    // Update is called once per frame
    void Update() { }

    /// <summary>
    /// Moves the banner to the position, sets the gameobject active and registers to each unit which is going there
    /// </summary>
    /// <param name="members"></param>
    /// <param name="position"></param>
    public void Deploy(Selectable[] members, Vector3 position)
    {
        transform.position = position;
        _members = new List<Selectable>(members);
        gameObject.SetActive(true);
        RegisterToMembers();

    }


    private void RegisterToMembers()
    {
        foreach (Selectable  selected in _members)
        {
            Unit unit = selected.gameObject.GetComponent<Unit>();

            unit.register(Unit.Actions.MOVEMENT_END, OnUnitActionChanges);


        }

    }

    public void OnUnitActionChanges(object obj)
    {
        Selectable select = ((GameObject)obj).GetComponent<Selectable>();
        _members.Remove(select);

        Unit unit = select.gameObject.GetComponent<Unit>();

        unit.unregister(Unit.Actions.MOVEMENT_END, OnUnitActionChanges);
        CheckDestroy();
    }


    private void CheckDestroy()
    {
        if (_members.Count == 0) Destroy(this.gameObject);
    }


    public static GameObject CreateBanner(Storage.Races race)
    {
        GameObject banner = null;
        switch (race)
        {
            case Storage.Races.ELVES:
                banner = Instantiate((GameObject)Resources.Load("Prefabs/Banners/Elves", typeof(GameObject)));
                break;
            case Storage.Races.MEN:
                banner =Instantiate((GameObject)Resources.Load("Prefabs/Banners/Humans", typeof(GameObject)));
                break;

        }

        return banner;
    }
}
