using UnityEngine;
using System.Collections;
using System;

namespace SceneLoader
{
    public class GameSceneDefaultLoader : MonoBehaviour
    {
        public Storage.Races _playerRace;
        public Storage.Races _iaRace;
        private GameObject informationObject = null;
        private GameInformation gameInfo;

        void Start()
        {
            LoadRequiredComponents();
            LoadSceneContext();
        }

        /// <summary>
        /// Retrieve or instantiate the required components
        /// </summary>
        private void LoadRequiredComponents()
        {
            if (!informationObject)
            {
                informationObject = GameObject.Find("GameInformationObject");
                if (!informationObject)
                {
                    informationObject = new GameObject("GameInformationObject");
                    informationObject.AddComponent<GameInformation>();
                }
                gameInfo = informationObject.GetComponent<GameInformation>();
            }

        }

        /// <summary>
        /// Load the hud and everything else using gameInformation
        /// </summary>
        private void LoadHUD()
        {
            gameInfo.SetPlayerRace(_playerRace);
            gameInfo.LoadHUD();
        }


        /// <summary>
        /// Public function to load different contexts
        /// </summary>
        public void LoadSceneContext() {

            if (_playerRace == _iaRace)
            {
                throw new Exception("Player and IA can't have the same race. Loading aborted");
            }

            
            LoadHUD();
        }
    }
}
