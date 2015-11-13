using UnityEngine;
using System.Collections;
using Utils;
using Managers;
namespace Managers
{
    public class SoundsManager: MonoBehaviour
    {

        public AudioSource efxSource;
        public AudioClip selectionSound;
        public AudioClip actionSound;


        void Start()
        {
            // extraido de la wiki de unity
            //if (Instance != this) Destroy(gameObject);
            //DontDestroyOnLoad(gameObject);

            //if(information == null) information = GameObject.Find("GameInformationObject").GetComponent<GameInformation>();
            //Register to selectable actions
            //selectionSound = (AudioClip)Resources.Load("Sounds/test-fx/civil-selected");
            //actionSound = (AudioClip)Resources.Load("Sounds/test-fx/civil-action");
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.SELECT, onUnitSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.MOVE, onUnitAction);
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
            efxSource.clip = selectionSound;

            //Play the clip.
            efxSource.Play();
        }


        public void onUnitSelected(System.Object obj)
        {
            //Set the clip of our efxSource audio source to the clip passed in as a parameter.
            efxSource.clip = selectionSound;

            //Play the clip.
            efxSource.Play();
        }

        public void onUnitAction(System.Object obj)
        {
            //Set the clip of our efxSource audio source to the clip passed in as a parameter.
            efxSource.clip = actionSound;

            //Play the clip.
            efxSource.Play();
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}