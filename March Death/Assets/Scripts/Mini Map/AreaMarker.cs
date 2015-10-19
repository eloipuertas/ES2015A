using UnityEngine;
using System.Collections;

public static class AreaMarker {

    private static int Height = 35, Width = 35, Corner = 10;

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
        Debug.Log(actor_cntr.x + " : " + actor_cntr.y + " : " + actor_cntr.z);

        box.xMin = actor_cntr.x - pix;
        box.xMax = actor_cntr.x + pix;
        box.yMin = Screen.height - actor_cntr.y - pix;
        box.yMax = Screen.height - actor_cntr.y + pix;

        return box;

    }

    public static Texture2D CreateTexture(Color borderColour)
    {
        Texture2D texToReturn = new Texture2D(Width, Height, TextureFormat.ARGB32, false);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (i == 0 || i == 1 || j == 0 || j == 1 || i == Width - 1 || i == Width - 2 || j == Height - 1
                    || j == Height - 2)
                {
                    texToReturn.SetPixel(i, j, borderColour);
                }
                else
                {
                    texToReturn.SetPixel(i, j, Color.clear);
                }
            }
        }
        
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (i == 0 || i == 1 || i == Width - 1 || i == Width - 2)
                {
                    if (j > Corner && j < Height - Corner) texToReturn.SetPixel(i, j, Color.clear);
                }
                else if (j == 0 || j == 1 || j == Width - 1 || j == Width - 2)
                {
                    if (i > Corner && i < Height - Corner) texToReturn.SetPixel(i, j, Color.clear);
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
