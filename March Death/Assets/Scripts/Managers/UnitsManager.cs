using UnityEngine;
using System.Collections;

public class UnitsManager : MonoBehaviour {

    Player player;
    UserInput inputs;
    private ArrayList _selectedUnits { get { return player.getSelectedObjects(); } }

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        inputs = GetComponent<UserInput>();

    }

    public void MoveTo(Vector3 point)
    {
        foreach (Selectable unit in _selectedUnits)
        {
            if (unit.GetComponent<IGameEntity>().info.isUnit)
                unit.GetComponent<Unit>().moveTo(point);
        }
        Debug.Log("Moving there");
    }

    public void AttackTo(IGameEntity enemy)
    {
        foreach (Selectable unit in _selectedUnits)
        {
            if (unit.GetComponent<IGameEntity>().info.isUnit && enemy.info.isUnit)
            {
                // so far we only can attack units
                unit.GetComponent<Unit>().attackTarget((Unit)enemy);
            }
        }
        Debug.Log("attacking");
    }

    // Update is called once per frame
    void Update () {
	
	}
}
