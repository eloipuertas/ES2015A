using UnityEngine;
using System.Collections;
using Utils;
using Managers;
namespace Managers
{
    public class SoundsManager: MonoBehaviour
    {

        private AudioPool selectionSoundPool;
        private AudioPool actionSoundPool;
        private AudioClip[] selectionSound;
        private AudioClip[] actionSound;


        void Start()
        {
            Setup();
            fakeSetup();

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
        public void onEntitytSelected(System.Object obj)
        {
            selectionSoundPool.Play(selectionSound[RandomChoice()]);
        }

        /// <summary>
        /// function to be triggered when a unit performs an action
        /// </summary>
        /// <param name="obj"></param>
        public void onUnitAction(System.Object obj)
        {
            actionSoundPool.Play(actionSound[RandomChoice()]);
        }

        /// <summary>
        /// Randomizes test sounds
        /// </summary>
        /// <returns></returns>
        private int RandomChoice()
        {
            return Random.Range(0, 3);
        }

        void OnDestroy()
        {
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.SELECT, onEntitytSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.MOVE, onUnitAction);
        }
    }
}