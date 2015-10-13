using UnityEngine;
using System.Collections;
using System;

public class ConstructionGrid : MonoBehaviour {

    private Vector2 dimensions;
    private ArrayList reservedPositions;

	void Start () {
        dimensions = new Vector2(5f, 5f);
        reservedPositions = new ArrayList();
        Debug.Log(discretizeMapCoords(new Vector3(-11.2f, 10f,12f)).ToString());
    }
	
    /// <summary>
    /// Returns the center of a row of the grid where the building will be placed
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
	public Vector3 discretizeMapCoords(Vector3 position)
    {
        Vector3 discretizedCoords = new Vector3();
        discretizedCoords.x = (float) Math.Floor(position.x / dimensions.x) * dimensions.x + dimensions.x / 2;
        discretizedCoords.z = (float)Math.Floor(position.z / dimensions.y) * dimensions.y + dimensions.y / 2;
        discretizedCoords.y = position.y;
        return discretizedCoords;
    }

    /// <summary>
    /// Sets the new grid dimensions
    /// </summary>
    /// <param name="newDimensions"></param>
    public void setNewGridDimensions(Vector2 newDimensions)
    {
        dimensions = newDimensions;
    }

    /// <summary>
    /// Used to reserve a place to prevent others to construct in
    /// </summary>
    /// <param name="discretizedPosition"></param>
    public void reservePosition(Vector3 buildingDiscretizedPosition)
    {
        if (isNewPositionAbleForConstrucction(buildingDiscretizedPosition))
        {
            reservedPositions.Add(buildingDiscretizedPosition);

            Debug.Log("---------------------------------------------------------------------------\nPosition reserved: ");

            for (int i = 0; i < reservedPositions.Count; i++)
            {
                Debug.Log(reservedPositions[i].ToString());
            }
        }
        else
        {
            Debug.Log("This position couldn't be reserved");
        }
        
    }

    /// <summary>
    /// Libreates the current discretized position
    /// </summary>
    /// <param name="discretizedPosition"></param>
    public void liberatePosition(Vector3 discretizedPosition)
    {
        reservedPositions.Remove(discretizedPosition);

        Debug.Log("---------------------------------------------------------------------------\nPosition erased: ");

        for (int i = 0; i < reservedPositions.Count; i++)
        {
            Debug.Log(reservedPositions[i].ToString());
        }
    }

    /// <summary>
    /// Detects if a position is flat enougth to construct in passed row
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool isFlatEnoughtForConstruction(Vector3 discretizedPosition)
    {
        return true;
    }

    /// <summary>
    /// Used to ask the construction grid if a discretized position is able to construct in.
    /// </summary>
    /// <param name="discretizedPosition"></param>
    /// <returns></returns>
    private bool isNewPositionAbleForConstrucction(Vector3 discretizedPosition)
    {
        //If this position is contained on the array return false
        if (reservedPositions.Contains(discretizedPosition))
        {
            return false;
        }

        //next check if the zone is flat enought for construction
        return isFlatEnoughtForConstruction(discretizedPosition);
    }
}
