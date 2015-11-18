using UnityEngine;
using System.Collections;

public static class MinimapOverlays
{

    private static int Height_Unit = 1, Width_Unit = 1, Corner_Unit = 180;
    private static int Height_Marker = 35, Width_Marker = 35, Corner_Marker = 10;

    /// <summary>
    /// Calculates the box surrounding the collider
    /// </summary>
    /// <returns>The box</returns>
    /// <param name="collider">The collider of the element</param>
    public static Rect CalculateBox(Collider collider, Camera cam)
    {
        Rect box = new Rect();
        //Calculate size of overlay based on the objects size
        Vector3 max = collider.bounds.max;
        Vector3 min = collider.bounds.min;

        Vector3 lowerTopLeft = cam.WorldToScreenPoint(new Vector3(min.x, min.y, max.z));
        Vector3 lowerTopRight = cam.WorldToScreenPoint(new Vector3(max.x, min.y, max.z));
        Vector3 lowerBottomLeft = cam.WorldToScreenPoint(new Vector3(min.x, min.y, min.z));
        Vector3 lowerBottomRight = cam.WorldToScreenPoint(new Vector3(max.x, min.y, min.z));

        Vector3 upperTopLeft = cam.WorldToScreenPoint(new Vector3(min.x, max.y, max.z));
        Vector3 upperTopRight = cam.WorldToScreenPoint(new Vector3(max.x, max.y, max.z));
        Vector3 upperBottomLeft = cam.WorldToScreenPoint(new Vector3(min.x, max.y, min.z));
        Vector3 upperBottomRight = cam.WorldToScreenPoint(new Vector3(max.x, max.y, min.z));


        box.xMin = Mathf.Min(lowerTopLeft.x, lowerTopRight.x, lowerBottomLeft.x, lowerBottomRight.x, upperTopLeft.x, upperTopRight.x, upperBottomRight.x, upperBottomLeft.x) - 5;
        box.xMax = Mathf.Max(lowerTopLeft.x, lowerTopRight.x, lowerBottomLeft.x, lowerBottomRight.x, upperTopLeft.x, upperTopRight.x, upperBottomRight.x, upperBottomLeft.x) + 5;
        box.yMin = Screen.height - Mathf.Max(lowerTopLeft.y, lowerTopRight.y, lowerBottomLeft.y, lowerBottomRight.y, upperTopLeft.y, upperTopRight.y, upperBottomRight.y, upperBottomLeft.y) - 15;
        box.yMax = Screen.height - Mathf.Min(lowerTopLeft.y, lowerTopRight.y, lowerBottomLeft.y, lowerBottomRight.y, upperTopLeft.y, upperTopRight.y, upperBottomRight.y, upperBottomLeft.y) + 5;
        return box;

    }

    /// <summary>
    /// Calculates the box surrounding the of the unit.
    /// </summary>
    /// <returns>The box</returns>
    /// <param name="cntr">The center of the unit</param>
    public static Rect CalculateBoxFromCntr(Vector3 cntr, Camera cam, int pix)
    {
        Rect box = new Rect();

        Vector3 actor_cntr = cam.WorldToViewportPoint(cntr);
        actor_cntr = cam.ViewportToScreenPoint(actor_cntr);
        //Debug.Log(actor_cntr.x + " : " + actor_cntr.y + " : " + actor_cntr.z);

        box.xMin = actor_cntr.x - pix;
        box.xMax = actor_cntr.x + pix;
        box.yMin = Screen.height - actor_cntr.y - pix;
        box.yMax = Screen.height -  actor_cntr.y + pix;

        return box;

    }

    /// <summary>
    /// Creates a texture for a unit.
    /// </summary>
    /// <param name="borderColour"></param>
    /// <returns></returns>
    public static Texture2D CreateTextureUnit(Color borderColour)
    {
        Texture2D texToReturn = new Texture2D(Width_Unit, Height_Unit, TextureFormat.ARGB32, false);

        for (int i = 0; i < Width_Unit; i++)
        {
            for (int j = 0; j < Height_Unit; j++)
            {
                if (i == 0 || i == 1 || j == 0 || j == 1 || i == Width_Unit - 1 || i == Width_Unit - 2 || j == Height_Unit - 1
                    || j == Height_Unit - 2)
                {
                    texToReturn.SetPixel(i, j, borderColour);
                }
                else
                {
                    texToReturn.SetPixel(i, j, Color.clear);
                }
            }
        }
        for (int i = 0; i < Width_Unit; i++)
        {
            for (int j = 0; j < Height_Unit; j++)
            {
                if (i == 0 || i == 1 || i == Width_Unit - 1 || i == Width_Unit - 2)
                {
                    if (j > Corner_Unit && j < Height_Unit - Corner_Unit) texToReturn.SetPixel(i, j, Color.clear);
                }
                else if (j == 0 || j == 1/* || j == Width_Unit - 1 || j == Width_Unit - 2*/)
                {
                    if (i > Corner_Unit && i < Height_Unit - Corner_Unit) texToReturn.SetPixel(i, j, Color.clear);
                }
            }

        }

        texToReturn.Apply();
        return texToReturn;
    }

    /// <summary>
    /// Creates a texture for the marker of the map.
    /// </summary>
    /// <param name="borderColour"></param>
    /// <returns></returns>
    public static Texture2D CreateTextureMarker(Color borderColour)
    {
        Texture2D texToReturn = new Texture2D(Width_Marker, Height_Marker, TextureFormat.ARGB32, false);

        for (int i = 0; i < Width_Marker; i++)
        {
            for (int j = 0; j < Height_Marker; j++)
            {
                if (i == 0 || i == 1 || j == 0 || j == 1 || j==2 || i == Width_Marker - 1 || i == Width_Marker - 2 || j == Height_Marker - 1 
                    || j == Height_Marker - 2 || j == Height_Marker - 3)
                {
                    texToReturn.SetPixel(i, j, borderColour);
                }
                else
                {
                    texToReturn.SetPixel(i, j, Color.clear);
                }
            }
        }

        texToReturn.Apply();
        return texToReturn;
    }

    /// <summary>
    /// Creates a texture for the marker of the map.
    /// </summary>
    /// <param name="borderColour"></param>
    /// <returns></returns>
    public static Texture2D CreateTextureAttackMarker(Color borderColour)
    {
        Texture2D texToReturn = new Texture2D(Width_Marker, Height_Marker, TextureFormat.ARGB32, false);

        for (int i = 0; i < Width_Marker; i++)
        {
            for (int j = 0; j < Height_Marker; j++)
            {
                if (i == 0 || i == 1 || j == 0 || j == 1 || j== 2 || i == Width_Marker - 1 || i == Width_Marker - 2 || j == Height_Marker - 1
                    || j == Height_Marker - 2 || j == Height_Marker - 3)
                {
                    texToReturn.SetPixel(i, j, borderColour);
                }
                else
                {
                    texToReturn.SetPixel(i, j, Color.clear);
                }
            }
        }

        for (int i = 0; i < Width_Marker; i++)
        {
            for (int j = 0; j < Height_Marker; j++)
            {
                if (i == 0 || i == 1 || i == Width_Marker - 1 || i == Width_Marker - 2)
                {
                    if (j > Corner_Marker && j < Height_Marker - Corner_Marker) texToReturn.SetPixel(i, j, Color.clear);
                }
                else if (j == 0 || j == 1 || j == Width_Marker - 1 || j == Width_Marker - 2)
                {
                    if (i > Corner_Marker && i < Height_Marker - Corner_Marker) texToReturn.SetPixel(i, j, Color.clear);
                }
            }

        }
        
        texToReturn.Apply();
        return texToReturn;
    }

    public static void UpdateTexture(Texture2D texture, float healthRatio)
    {
        texture.Apply();
    }
}