using UnityEngine;
using Utils;
using Storage;

namespace Managers
{
    public class SoundsManager: MonoBehaviour
    {

        private AudioPool _unitsAudioPool;
        private AudioPool _buildingsAudioPool;


        void Start()
        {
            Setup();
        }

        /// <summary>
        /// Setups the SoundsManager
        /// </summary>
        void Setup()
        {
            _buildingsAudioPool = new AudioPool(this.gameObject, 3);
            _unitsAudioPool = new AudioPool(this.gameObject, 5);
           

            // suscribes the audio managet to main events 
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.SELECT, onEntitytSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.registerForAll(SelectionManager.Actions.MOVE, onUnitAction);
        }


        /// <summary>
        /// function to be triggered when an entity is selected
        /// </summary>
        /// <param name="obj"></param>
        public void onEntitytSelected(object obj)
        {
            Selectable  select = (Selectable)obj;
            //IGameEntity entity = gameObject.GetComponent<IGameEntity>();

            if (select.entity.info.isBuilding)
            {
                _buildingsAudioPool.Play(Sounds.get.Clip(select.entity.getType<BuildingTypes>(), Sounds.SoundType.SELECTION));
            }
            else
                _unitsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.UNIT, Sounds.SoundType.SELECTION));
        }

        /// <summary>
        /// function to be triggered when a unit performs an action
        /// </summary>
        /// <param name="obj"></param>
        public void onUnitAction(object obj)
        {
            _unitsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.UNIT, Sounds.SoundType.ACTION));
        }

        /// <summary>
        /// function to trigger a sound when building is created
        /// </summary>
        /// <param name="bType"></param>
        public void onBuildingCreated(BuildingTypes bType)
        {

            _buildingsAudioPool.Play(Sounds.get.Clip(bType, Sounds.SoundType.CREATION));
        }

        /// <summary>
        /// function to trigger a sound when unit is created
        /// </summary>
        /// <param name="bType"></param>
        public void onUnitCreated()
        {

            _unitsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.UNIT, Sounds.SoundType.SELECTION)); // is selection sound too
        }


        /// <summary>
        /// function to trigger a sound when building is destroyed
        /// </summary>
        /// <param name="bType"></param>
        public void onBuildingDestroyed()
        {
            _buildingsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.BUILDING, Sounds.SoundType.DEAD));
        }


        /// <summary>
        /// function to trigger a sound when unit is dead
        /// </summary>
        /// <param name="bType"></param>
        public void onUnitDead()
        {
            _unitsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.UNIT, Sounds.SoundType.DEAD));

        }



        void OnDestroy()
        {
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.SELECT, onEntitytSelected);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.ATTACK, onUnitAction);
            Subscriber<SelectionManager.Actions, SelectionManager>.get.unregisterFromAll(SelectionManager.Actions.MOVE, onUnitAction);
        }
    }
}