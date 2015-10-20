using UnityEngine;
using System.Collections;

public class UnitsManager : MonoBehaviour {

    Player player;
    UserInput inputs;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        inputs = GetComponent<UserInput>();

    }

    public void moveUnits(Vector3 point) {
        ArrayList units = player.getSelectedObjects();
        foreach (Selectable unit in units) {
            if (unit.GetComponent<IGameEntity>().info.isUnit) {
                unit.GetComponent<Unit>().moveTo(point);

            }

        }

    }

    // Update is called once per frame
    void Update () {
	
	}
}
