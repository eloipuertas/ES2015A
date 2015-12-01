using UnityEngine;
using System.Collections;
using System;

public static class MMapAnim {

    public static Texture2D[] ANIMATION_ATTACK = MMapAnim.GetAttackAnim(Color.red);
    public static Texture2D[] ANIMATION_CREATE = MMapAnim.GetCreateAnim(Color.green);

    public static Texture2D[] GetAttackAnim(Color color)
    {
        int num_text = 3;
        int initial_size = 32;

        Texture2D[] array_text = new Texture2D[num_text];

        for (int i = 0; i < num_text; i++)
        {
            array_text[i] = GetAttackText(initial_size/2, 4, Color.red);
        }

        return array_text;
    }

    private static Texture2D GetAttackText(int size , int corner, Color color)
    {
        Texture2D texToReturn = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == 0 || i == 1 || j == 0 || j == 1 || i == size - 1 || i == size - 2 || j == size - 1
                    || j == size - 2)
                {
                    texToReturn.SetPixel(i, j, color);
                }
                else
                {
                    texToReturn.SetPixel(i, j, Color.clear);
                }
            }

        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == 0 || i == 1 || i == size - 1 || i == size - 2)
                {
                    if (j > corner && j < size - corner) texToReturn.SetPixel(i, j, Color.clear);
                }
                else if (j == 0 || j == 1 || j == size - 1 || j == size - 2)
                {
                    if (i > corner && i < size - corner) texToReturn.SetPixel(i, j, Color.clear);
                }
            }

        }

        texToReturn.Apply();
        return texToReturn;
    }

    public static Texture2D[] GetCreateAnim(Color color)
    {
        int num_text = 3;
        int initial_size = 32;

        Texture2D[] array_text = new Texture2D[num_text];

        for (int i = 0; i < num_text; i++)
        {
            array_text[i] = GetCreateText(initial_size / (i+1), 4, Color.yellow);
            array_text[i].Apply();
        }

        return array_text;
    }

    private static Texture2D GetCreateText(int size, int corner, Color color)
    {
        int a = size / 2;
        int r_2 = (int)Mathf.Pow((size-1)/2 , 2);

        Texture2D texToReturn = new Texture2D(size, size, TextureFormat.ARGB32, false);
       

        texToReturn.Apply();
        return texToReturn;
    }

}
