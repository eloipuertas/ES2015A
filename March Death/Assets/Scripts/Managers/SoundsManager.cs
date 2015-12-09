using UnityEngine;
using Utils;
using Storage;

namespace Managers
{
    public class SoundsManager: MonoBehaviour
    {

        private AudioPool _unitsAudioPool;
        private AudioPool _buildingsAudioPool;
        private SelectionManager _sManager;


        void Awake()
        {
            _buildingsAudioPool = new AudioPool(this.gameObject, 3);
            _unitsAudioPool = new AudioPool(this.gameObject, 3);
        }

        void Start()
        {
            _sManager = GetComponent<SelectionManager>();
            Register();
        }

        /// <summary>
        /// Setups the SoundsManager
        /// </summary>
        private void Register()
        {

            // suscribes the audio managet to main events 
            _sManager.register(SelectionManager.Actions.SELECT, onEntitytSelected);
            _sManager.register(SelectionManager.Actions.ATTACK, onUnitAction);
            _sManager.register(SelectionManager.Actions.MOVE, onUnitAction);
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

        /// <summary>
        /// function to trigger a sound when building trap explorer recruited as worker
        /// </summary>
        /// <param name="bType"></param>
        public void onExplorerTrapped()
        {
            _buildingsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.BUILDING, Sounds.SoundType.TRAP));
        }

        /// <summary>
        /// function to trigger a sound when building could not trap a explorer because is full.
        /// </summary>
        /// <param name="bType"></param>
        public void onFullHouse()
        {
            _buildingsAudioPool.Play(Sounds.get.RandomClip(Sounds.SoundSource.BUILDING, Sounds.SoundType.FULLHOUSE));
        }


        void OnDestroy()
        {
            _sManager.unregister(SelectionManager.Actions.SELECT, onEntitytSelected);
            _sManager.unregister(SelectionManager.Actions.ATTACK, onUnitAction);
            _sManager.unregister(SelectionManager.Actions.MOVE, onUnitAction);
        }
    }
}