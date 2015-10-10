using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour
{
    private Player player;
    //Should be better to create a constants class or structure
    private Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        //player = transform.root.GetComponentInChildren<Player>();
    }

    void Update()
    {

        MouseActivity();

    }
    private void MouseActivity()
    {
        if (Input.GetMouseButtonDown(0)) LeftMouseClick();
        else if (Input.GetMouseButtonDown(1)) RightMouseClick();
    }

    private void LeftMouseClick()
    {
        if (player.isCurrently(Player.status.PLACING_BUILDING)) // we are locating a building
        {
            GetComponent<BuildingsManager>().placeBuilding();
        }
        else // we are doing something else
        {
            GameObject hitObject = FindHitObject();
            Vector3 hitPoint = FindHitPoint();

            if (hitObject)
            {

                Selectable selectedObject = hitObject.GetComponent<Selectable>();
                // We just be sure that is a selectable object
                if (selectedObject)
                {
                    selectedObject.Select(player);
                }
            }
            else if (hitPoint != this.invalidPosition)
            {
                /* TODO check if click is not out of bounds ( perhaps the click is in the HUD ) */

            }
            else
            {
                //TODO where is the hit???
            }
        }
    }

    public GameObject FindHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
        return null;
    }

    public  Vector3 FindHitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.point;
        return this.invalidPosition;
    }

    private void RightMouseClick()
    {
        if (player.SelectedObject)
        {
            player.SelectedObject.Deselect();
            player.SelectedObject = null;
        }
        else if(player.isCurrently(Player.status.PLACING_BUILDING))
        {
            GetComponent<BuildingsManager>().cancelPlacing();
        }
    }

}