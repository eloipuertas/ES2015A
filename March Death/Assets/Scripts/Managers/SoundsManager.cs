using UnityEngine;
using System.Collections;
using Utils;

public class SoundsManager : SingletonMono<SoundsManager> {

    public AudioSource efxSource;
    public AudioSource musicSource;
    public AudioClip testSound;


    void Awake()
    {
        // extraido de la wiki de unity
        if (Instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        //Register to selectable actions
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.SELECTED, onUnitSelected);
    }



    //Used to play single sound clips.
    public void PlaySingle(AudioClip clip)
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        efxSource.clip = clip;

        //Play the clip.
        efxSource.Play();
    }

    //Used to play single sound clips.
    public void PlaySingle()
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        efxSource.clip = testSound;

        //Play the clip.
        efxSource.Play();
    }


    public void onUnitSelected(System.Object obj)
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        efxSource.clip = testSound;

        //Play the clip.
        efxSource.Play();
    }
    // Update is called once per frame
    void Update () {
	
	}
}
