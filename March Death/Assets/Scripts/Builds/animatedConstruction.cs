using UnityEngine;
using System.Collections;

using System;
public class animatedConstruction : MonoBehaviour {

    private Renderer terrat;
    //private Renderer terra;
    void Awake()
    {
        terrat = GameObject.Find("Terrat").GetComponent<MeshRenderer>();
        
    }
    // Use this for initialization
    void Start()
    {
        //terra = GameObject.Find("Terra").GetComponent<MeshRenderer>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (!(r.name.Equals("Terra")))
                r.enabled = false;


        }
        StartCoroutine(Iniciar());

    }
    IEnumerator Iniciar()
    {
        
        yield return StartCoroutine(Accion1());               
        print("esto debe aparecer 5 segundos mas tarde");
        terrat.enabled = true;
    }

    IEnumerator Accion1()
    {

        print("iniciando accion 1");
        yield return new WaitForSeconds(5);
    }
    // Update is called once per frame
    void Update () {
        //print("esto debe aparecer 5 segundos mas tarde");

        //MeshRenderer showZone = this.GetComponent<MeshRenderer>();
        //showZone = GameObject.Find("Terrat").GetComponent<MeshRenderer>();
        //showZone.enabled = true;

    }
}
