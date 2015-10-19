using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class ConstructionGrid : MonoBehaviour
{
    private Vector2 dimensions;
    private ArrayList reservedPositions;
    private const float DIFERENCE_OF_HEIGHTS_TOLERANCE = 3f;

    void Start()
    {
        dimensions = new Vector2(20f, 20f);
        reservedPositions = new ArrayList();
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Returns the center of a row of the grid where the building will be placed
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 discretizeMapCoords(Vector3 position)
    {
        Vector3 discretizedCoords = new Vector3();
        discretizedCoords.x = (float)Math.Floor(position.x / dimensions.x) * dimensions.x + dimensions.x / 2;
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
        if (newDimensions.x <= 0 || newDimensions.y <= 0)
        {
            throw new InvalidOperationException("New dimensions must be bigger than zero");
        }

        dimensions = newDimensions;
    }

    /// <summary>
    /// Used to reserve a place to prevent others to construct in
    /// </summary>
    /// <param name="discretizedPosition"></param>
    public void reservePosition(Vector3 buildingDiscretizedPosition)
    {
        Vector2 PositionToVector2 = new Vector2(buildingDiscretizedPosition.x, buildingDiscretizedPosition.z);
        if (!reservedPositions.Contains(PositionToVector2))
        {
            reservedPositions.Add(PositionToVector2);
        }
    }

    /// <summary>
    /// Libreates the current discretized position
    /// </summary>
    /// <param name="discretizedPosition"></param>
    public void liberatePosition(Vector3 discretizedPosition)
    {
        reservedPositions.Remove(new Vector2(discretizedPosition.x, discretizedPosition.z));
    }

    private float getPointHeight(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit))
            return hit.point.y; 
        if (Physics.Raycast(point, Vector3.up, out hit))
            return hit.point.y;
        return float.NegativeInfinity;
    }

    /// <summary>
    /// Detects if a position is flat enougth to construct in passed row
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool isFlatEnoughtForConstruction(Vector3 discretizedPosition)
    {
        float centerHeight = discretizedPosition.y;
        float topLeftHeight = getPointHeight(new Vector3(discretizedPosition.x - dimensions.x / 2, discretizedPosition.y, discretizedPosition.z - dimensions.y / 2));
        float topRightHeight = getPointHeight(new Vector3(discretizedPosition.x - dimensions.x / 2, discretizedPosition.y, discretizedPosition.z + dimensions.y / 2));
        float BottomLeftHeight = getPointHeight(new Vector3(discretizedPosition.x + dimensions.x / 2, discretizedPosition.y, discretizedPosition.z - dimensions.y / 2));
        float BottomRightHeight = getPointHeight(new Vector3(discretizedPosition.x + dimensions.x / 2, discretizedPosition.y, discretizedPosition.z + dimensions.y / 2));

        var heights = new float[] { centerHeight, topLeftHeight, topRightHeight, BottomLeftHeight, BottomRightHeight };
        float max_height = heights.Max();
        float min_height = heights.Min();
        float difference = max_height - min_height;
        
        return difference < DIFERENCE_OF_HEIGHTS_TOLERANCE;
    }

    /// <summary>
    /// Used to ask the construction grid if a discretized position is able to construct in.
    /// </summary>
    /// <param name="discretizedPosition"></param>
    /// <returns></returns>
    public bool isNewPositionAbleForConstrucction(Vector3 discretizedPosition)
    {
        //If this position is contained on the array return false
        if (reservedPositions.Contains(new Vector2(discretizedPosition.x, discretizedPosition.z)))
        {
            Debug.Log("This position is already reserved");
            return false;
        }

        //next check if the zone is flat enought for construction
        return isFlatEnoughtForConstruction(discretizedPosition);
    }
}
