using UnityEngine;
using System.Collections;

public static class SelectionOverlay{

    private static int Height = 100, Width = 100, Corner = 30;
    private static int HealthHeight = 10;

    private static Color BorderColour = Color.white;
    private static Color HealthColour = Color.green;

    /// <summary>
    /// Calculates the box surrounding the collider
    /// </summary>
    /// <returns>The box</returns>
    /// <param name="collider">The collider of the element</param>
    public static Rect CalculateBox(Collider collider)
    {
        Rect box = new Rect();
        //Calculate size of overlay based on the objects size
        Vector3 max = collider.bounds.max;
        Vector3 min = collider.bounds.min;

        Vector3 lowerTopLeft = Camera.main.WorldToScreenPoint(new Vector3(min.x, min.y, max.z));
        Vector3 lowerTopRight = Camera.main.WorldToScreenPoint(new Vector3(max.x, min.y, max.z));
        Vector3 lowerBottomLeft = Camera.main.WorldToScreenPoint(new Vector3(min.x, min.y, min.z));
        Vector3 lowerBottomRight = Camera.main.WorldToScreenPoint(new Vector3(max.x, min.y, min.z));

        Vector3 upperTopLeft = Camera.main.WorldToScreenPoint(new Vector3(min.x, max.y, max.z));
        Vector3 upperTopRight = Camera.main.WorldToScreenPoint(new Vector3(max.x, max.y, max.z));
        Vector3 upperBottomLeft = Camera.main.WorldToScreenPoint(new Vector3(min.x, max.y, min.z));
        Vector3 upperBottomRight = Camera.main.WorldToScreenPoint(new Vector3(max.x, max.y, min.z));


        box.xMin = Mathf.Min(lowerTopLeft.x, lowerTopRight.x, lowerBottomLeft.x, lowerBottomRight.x, upperTopLeft.x, upperTopRight.x, upperBottomRight.x, upperBottomLeft.x) - 5;
        box.xMax = Mathf.Max(lowerTopLeft.x, lowerTopRight.x, lowerBottomLeft.x, lowerBottomRight.x, upperTopLeft.x, upperTopRight.x, upperBottomRight.x, upperBottomLeft.x) + 5;
        box.yMin = Screen.height - Mathf.Max(lowerTopLeft.y, lowerTopRight.y, lowerBottomLeft.y, lowerBottomRight.y, upperTopLeft.y, upperTopRight.y, upperBottomRight.y, upperBottomLeft.y) - 15;
        box.yMax = Screen.height - Mathf.Min(lowerTopLeft.y, lowerTopRight.y, lowerBottomLeft.y, lowerBottomRight.y, upperTopLeft.y, upperTopRight.y, upperBottomRight.y, upperBottomLeft.y) + 5;
        return box;

    }

	public static Texture2D CreateTexture(bool ownUnit)
    {
        Texture2D texToReturn = new Texture2D(Width, Height, TextureFormat.ARGB32, false);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
				//Draw all selectionOverlay
				if (ownUnit && (i == 0 || i == 1 || j == 0 || j == 1 || i == Width - 1 || i == Width - 2 || j == Height - 1
				    || j == Height - 2 || j == Height - HealthHeight || j == Height - HealthHeight - 1)) 
				{
					texToReturn.SetPixel(i, j, BorderColour);
				}

				//Draw only health
				else if (!ownUnit && ((((i == 0  || i == 1 || i == Width - 1 || i == Width - 2) && (j > Height - HealthHeight )) 
				     || (j == Height - 1 || j == Height - 2 || j == Height - HealthHeight || j == Height - HealthHeight - 1))))
				{
					texToReturn.SetPixel(i, j, BorderColour);
				}

                else if (j > Height - HealthHeight)
                {
                    texToReturn.SetPixel(i, j, HealthColour);
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
                else if (j == 0 || j == 1/* || j == Width - 1 || j == Width - 2*/)
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
        for (int i = 2; i < Width - 2; i++)
        {
            for (int j = Height - HealthHeight + 1; j < Height - 2; j++)
            {
                if ((float)i / (float)Width < healthRatio)
                {
                    texture.SetPixel(i, j, HealthColour);
                }
                else
                {
                    texture.SetPixel(i, j, Color.red);
                }
            }
        }
        texture.Apply();
    }
}

