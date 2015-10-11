using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;

public class animatedConstructionElf : MonoBehaviour {
    
    private Renderer[] renderers;
    private List<Renderer> llista_1aFase;
    private List<Renderer> llista_2aFase;
    private List<Renderer> llista_3aFase;
    private Animation anim;

    void Awake() {}

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animation>();
        anim.Stop();

        llista_1aFase = new List<Renderer>();
        llista_2aFase = new List<Renderer>();
        llista_3aFase = new List<Renderer>();

        llista_1aFase.Add(GameObject.Find("Terra").GetComponent<MeshRenderer>());
        llista_1aFase.Add(GameObject.Find("Bigas").GetComponent<MeshRenderer>());
        llista_1aFase.Add(GameObject.Find("TroncosA").GetComponent<MeshRenderer>());
        llista_1aFase.Add(GameObject.Find("TroncosB").GetComponent<MeshRenderer>());
        llista_1aFase.Add(GameObject.Find("TroncosC").GetComponent<MeshRenderer>());

        llista_2aFase.Add(GameObject.Find("TerraEdifici").GetComponent<MeshRenderer>());
        llista_2aFase.Add(GameObject.Find("BarandillaA").GetComponent<MeshRenderer>());
        llista_2aFase.Add(GameObject.Find("Escala").GetComponent<MeshRenderer>());
        
        llista_3aFase.Add(GameObject.Find("Apost").GetComponent<MeshRenderer>());
        llista_3aFase.Add(GameObject.Find("BigaTerrat").GetComponent<MeshRenderer>());
        llista_3aFase.Add(GameObject.Find("BarandillaB").GetComponent<MeshRenderer>());
        llista_3aFase.Add(GameObject.Find("CartelBarras").GetComponent<MeshRenderer>());
        llista_3aFase.Add(GameObject.Find("Cartel").GetComponent<MeshRenderer>());
        
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer i in renderers)
        {
            i.enabled = false;
        }
        foreach (Renderer i in llista_1aFase)
        {
            i.enabled = true;
        }

        StartCoroutine(Iniciar());

    }
    IEnumerator Iniciar()
    {        
        yield return StartCoroutine(Accion1());
        print("esto debe aparecer 30 segundos mas tarde");
        foreach (Renderer i in llista_2aFase)
        {
            i.enabled = true;
        }
        yield return StartCoroutine(Accion1());
        print("esto debe aparecer 60 segundos mas tarde");
        foreach (Renderer i in llista_3aFase)
        {
            i.enabled = true;
        }
        yield return StartCoroutine(Accion1());
        print("esto debe aparecer 90 segundos mas tarde");
        foreach (Renderer i in renderers)
        {
            i.enabled = true;
        }
        anim.Play();
    }
    
    private IEnumerator Accion1()
    {
        print("iniciando accion 1");
        yield return new WaitForSeconds(10);
    }

    // Update is called once per frame
    void Update () {

    }
}
