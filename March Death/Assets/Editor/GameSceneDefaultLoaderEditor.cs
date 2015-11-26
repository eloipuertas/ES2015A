using UnityEditor;
using SceneLoader;
using UnityEngine;
using System;

/// <summary>
/// Class to create a custom inspector for the GameSceneDefaultLoader
/// </summary>
[CustomEditor(typeof(GameSceneDefaultLoader))]
public class GameSceneDefaultLoaderEditor : Editor {

    private string[] nombres = new[] { "Elves", "Human" };
    private Storage.Races[] elecciones = new[] { Storage.Races.ELVES, Storage.Races.MEN };
    private int eleccionPlayer = 0;
    private int eleccionIa = 1;
    private Storage.Races razaSeleccionada;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameSceneDefaultLoader sceneLoader = (GameSceneDefaultLoader)target;
        if (GUILayout.Button("Load Extra Units"))
        {
            //TODO
            
            sceneLoader.LoadExtraUnits();
        }
    }
}
