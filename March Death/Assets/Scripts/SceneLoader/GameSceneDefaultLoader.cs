using UnityEngine;
using System.Collections;
using System;

namespace SceneLoader
{
    public class GameSceneDefaultLoader : MonoBehaviour
    {
        public Storage.Races _playerRace = Storage.Races.ELVES;
        private Storage.Races _iaRace = Storage.Races.MEN;

        public int CreateXEntities = 0;
        public Storage.UnitTypes _unitType = Storage.UnitTypes.HERO;


        private GameObject informationObject = null;
        
        private GameInformation gameInfo;
        private GameObject _place;
        private Player player;

        void Awake(){}

        void Start()
        {
            if (LoadRequiredComponents()) LoadSceneContext();
        }

        /// <summary>
        /// Retrieve or instantiate the required components
        /// </summary>
        private bool LoadRequiredComponents()
        {
            //Checks if informationObject exists
            if (!informationObject)
            {
                informationObject = GameObject.Find("GameInformationObject");
                if (informationObject) return false;

                informationObject = new GameObject("GameInformationObject");
                informationObject.AddComponent<GameInformation>();
                gameInfo = informationObject.GetComponent<GameInformation>();
                gameInfo.setGameMode(GameInformation.GameMode.CAMPAIGN);
            }

            return true;

        }

        /// <summary>
        /// Load the hud and everything else using gameInformation
        /// </summary>
        private void LoadHUD()
        {
            gameInfo.SetPlayerRace(_playerRace);
            // NOTE: This would load a 2nd HUD
            //gameInfo.LoadHUD();
        }


        /// <summary>
        /// Public function to load different contexts
        /// </summary>
        public void LoadSceneContext() {

            if (_playerRace == _iaRace)
            {
                _iaRace = Storage.Races.ELVES;
            }

            
            LoadHUD();            
        }

        public void LoadExtraUnits()
        {
            if (CreateXEntities > 0)
            {
                _place = GameObject.Find("PlayerHero");
                player = (Player)BasePlayer.getOwner(_playerRace);

                Vector3 _basePosition = new Vector3(659f, 79f, 835f);
                Vector3 position = _basePosition;
                float step = 2f;
                Storage.Info info = Storage.Info.get;
                for (int i = 0; i < CreateXEntities; i++)
                {

                    if (i % 10 == 0)
                    {
                        position.z += step;
                        position.x = _basePosition.x;
                    }
                    else
                        position.x += step;

                    player.addEntity(info.createUnit(_playerRace, _unitType, position, new Quaternion(0, 0, 0, 0)).GetComponent<IGameEntity>());
                }
            }
        }


    }
}
