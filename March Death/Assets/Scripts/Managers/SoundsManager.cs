using UnityEngine;
using Utils;
using System.Collections.Generic;

namespace Managers
{
    public class SoundsManager: MonoBehaviour
    {

        private AudioPool selectionSoundPool;
        private AudioPool actionSoundPool;
        private AudioClip[] selectionSound;
        private AudioClip[] actionSound;
        private Dictionary<Tuple<Storage.BuildingTypes, string>, AudioClip> BuildingsSounds;


        void Start()
        {
            Setup();
            fakeSetup();
         //   LoadSoundLibrary();
        }


        private void LoadSoundLibrary()
        {
            /*
            Object[] assets = Resources.LoadAll("Sounds/Buildings", typeof(AudioClip));
            foreach(AudioClip audio in assets)
            {
                Debug.Log(audio.name);
                string[] name = audio.name.Split('-');
                string type = name[0], action = name[1];
                sounds.Add(new Tuple<string, string>(type, action), audio);

            }
            */
        }

        /// <summary>
        /// Setups the SoundsManager
        /// </summary>
        void Setup()
        {
            selectionSoundPool = new AudioPool(this.gameObject, 3);
            actionSoundPool = new AudioPool(this.gameObject, 3);

            // suscribes the audio managet to main events 
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.SELECT, onEntitytSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.MOVE, onUnitAction);
        }

        /// <summary>
        /// Adds test sounds
        /// </summary>
        void fakeSetup()
        {
            selectionSound = new AudioClip[3];
            actionSound = new AudioClip[3];

            //Register to selectable actions
            selectionSound[0] = (AudioClip)Resources.Load("Sounds/test-fx/civil-selected");
            selectionSound[1] = (AudioClip)Resources.Load("Sounds/test-fx/hero-selected");
            selectionSound[2] = (AudioClip)Resources.Load("Sounds/test-fx/hero-selected-2");

            actionSound[0] = (AudioClip)Resources.Load("Sounds/test-fx/civil-action");
            actionSound[1] = (AudioClip)Resources.Load("Sounds/test-fx/hero-action");
            actionSound[2] = (AudioClip)Resources.Load("Sounds/test-fx/hero-action-2");
        }


        /// <summary>
        /// function to be triggered when an entity is selected
        /// </summary>
        /// <param name="obj"></param>
        public void onEntitytSelected(object obj)
        {
            Selectable selected = (Selectable)obj;
            if(selected.entity.info.isBuilding)
            {
                Storage.BuildingInfo info = (Storage.BuildingInfo) selected.entity.info;
            }
            selectionSoundPool.Play(selectionSound[RandomChoice()]);
        }

        /// <summary>
        /// function to be triggered when a unit performs an action
        /// </summary>
        /// <param name="obj"></param>
        public void onUnitAction(object obj)
        {
            actionSoundPool.Play(actionSound[RandomChoice()]);
        }

        /// <summary>
        /// Randomizes test sounds
        /// </summary>
        /// <returns></returns>
        private int RandomChoice()
        {
            return UnityEngine.Random.Range(0, 3);
        }

        void OnDestroy()
        {
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.SELECT, onEntitytSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.MOVE, onUnitAction);
        }
    }
}