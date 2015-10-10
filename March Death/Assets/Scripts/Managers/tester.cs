using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class tester : MonoBehaviour {

    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => { onClick(); });
    }
    public void onClick()
    {
        GameObject.Find("GameController").GetComponent<BuildingsManager>().createBuilding("elf-farm");
        Debug.Log("click!");

    }
}
