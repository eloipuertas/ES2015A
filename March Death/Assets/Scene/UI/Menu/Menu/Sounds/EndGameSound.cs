using UnityEngine;
using System.Collections;

public class EndGameSound : MonoBehaviour {
    AudioSource source1;
    AudioSource source2;
    private AudioSource[] sources;
    public GameObject playlistObj;

    void Start () {
        sources = (AudioSource[])playlistObj.GetComponents(typeof(AudioSource));
        source1 = sources[0];
        source1.Pause();
        source2 = sources[1];
    }
	
	// Update is called once per frame
	void Update () {
        
	}
}
