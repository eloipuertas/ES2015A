using UnityEngine;
using System.Collections;
using System;

namespace SceneLoader
{
    public class GameSceneDefaultLoader : MonoBehaviour
    {
        public Storage.Races _playerRace = Storage.Races.ELVES;
        private Storage.Races _iaRace = Storage.Races.MEN;

        private GameObject informationObject = null;
        private GameInformation gameInfo;

        void Start()
        {
            if (LoadRequiredComponents()) LoadSceneContext();
        }

        /// <summary>
        /// Retrieve or instantiate the required components
        /// </summary>
        private bool LoadRequiredComponents()
        {
            if (!informationObject)
            {
                informationObject = GameObject.Find("GameInformationObject");
                if (informationObject) return false;

                informationObject = new GameObject("GameInformationObject");
                informationObject.AddComponent<GameInformation>();
                gameInfo = informationObject.GetComponent<GameInformation>();
            }

            return true;

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
                _iaRace = Storage.Races.ELVES;
                //throw new Exception("Player and IA can't have the same race. Loading will fail");
            }

            
            LoadHUD();
        }
    }
}
