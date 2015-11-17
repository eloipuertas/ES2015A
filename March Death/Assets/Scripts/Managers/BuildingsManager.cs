﻿using UnityEngine;
using System.Collections;
using Utils;
namespace Managers
{
    public class BuildingsManager : MonoBehaviour
    {
        public enum Place { ABLE, NOT_ABLE }
        private Place _currentPlace = Place.ABLE;
        public Place currentPlace { get { return _currentPlace; } }
        private Player _player;
        private UserInput _inputs;
        public Player Player { set { _player = value; } }
        public UserInput Inputs { set { _inputs = value; } }

        private CursorManager cursor;
        private ConstructionGrid grid;
        private Color red = Color.red;
        private Color green = Color.green;
        private struct NewBuilgind
        {
            public GameObject ghost;
            public GameObject building;
            public Storage.Races race;
            public Storage.BuildingTypes type;
            public bool placing;
            public Material material;

        }

        private NewBuilgind _newBuilding;
        float yoffset = 1f;

        // Use this for initialization
        void Start()
        {
            _player = GetComponent<Player>();
            _inputs = GetComponent<UserInput>();
            grid = GetComponent<ConstructionGrid>();
            cursor = CursorManager.Instance;
            // alpha components for the colors
            red.a = 0.5f;
            green.a = 0.5f;
            InitBuildingStruct();

        }

        private void InitBuildingStruct()
        {
            _newBuilding.placing = false;
            _newBuilding.ghost = null;
        }

        // Update is called once per frame
        void Update()
        {
            if (_newBuilding.placing)
            {
                relocate();
            }

        }


        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void createBuilding(Storage.Races race, Storage.BuildingTypes type )
        {
            if (!_newBuilding.placing)
            {
                _newBuilding.race = race;
                _newBuilding.type = type;
                _newBuilding.ghost = CreateGhostBuilding(race, type);
                _newBuilding.material = _newBuilding.ghost.GetComponent<Renderer>().material;
                _newBuilding.placing = true;
                _player.setCurrently(Player.status.PLACING_BUILDING);
            }

        }


        /// <summary>
        /// Returns the ghost building of the specified type
        /// </summary>
        /// <param name="race"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private GameObject CreateGhostBuilding(Storage.Races race, Storage.BuildingTypes type)
        {
            //TODO : (hermetico) change shared ghost
            GameObject ghost = (GameObject)Resources.Load("Prefabs/Buildings/Resources/GHOST_Elf-Farm", typeof(GameObject));
            ghost = (GameObject)Instantiate(ghost, new Vector3(0, 0, 0), Quaternion.identity);
            return ghost;

        }


        /// <summary>
        /// Returns the building of the spcedified type
        /// </summary>
        /// <param name="race"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private GameObject CreateFinalBuilding(Storage.Races race, Storage.BuildingTypes type)
        {
            return Storage.Info.get.createBuilding(race, type);
        }


        /// <summary>
        /// Checks if is valid locatoin through Constructiongrid
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool checkLocation(Vector3 location)
        {
            bool check = false;

            check = grid.isNewPositionAbleForConstrucction(location);

            return check;

        }

        /// <summary>
        /// Places the building, checking if is a suitable place
        /// </summary>
        public bool placeBuilding()
        {
            Vector3 newDestination = GetNewDestination();
            // if is not a vaild point, the building remains quiet
            if (newDestination == _inputs.invalidPosition) return false;

            // alter the color if is not a valid location
            if (checkLocation(newDestination))
            {

                GameObject finalBuilding = CreateFinalBuilding(_newBuilding.race, _newBuilding.type);
                //TODO : (hermetico) restar recursos necesarios para crear el building
                grid.reservePosition(newDestination);
                finalBuilding.transform.position = newDestination;

                //TODO : check another way to get the IGameEntity
                
               // IGameEntity entity = finalBuilding.gameObject.GetComponent<IGameEntity>(); // Esto no iria así ? (Ferran)
                IGameEntity entity = (IGameEntity)finalBuilding.GetComponent<Unit>();
                _player.addEntity(entity);

                // remaining operations
                _finishPlacing();

                return true;
            }
            else
                return false;


        }

        /// <summary>
        /// Cancel the placing of the building and returns player to idle status
        /// </summary>
        public void cancelPlacing()
        {
            if (_newBuilding.placing)
            {
                // remaining operations
                _finishPlacing();
            }
        }


        /// <summary>
        /// Common operations when placing or cancelling placing
        /// </summary>
        private void _finishPlacing()
        {
            Destroy(_newBuilding.ghost);
            _newBuilding.placing = false;

        }


        /// <summary>
        /// returns a vector with the position after apply an offset and discretyze the position
        /// </summary>
        /// <returns></returns>
        private Vector3 GetNewDestination()
        {
            // 1. getPoint
            Vector3 toLocation = _inputs.FindTerrainHitPoint();
            // let the buildings not to fall down
            toLocation.y += yoffset;
            // 2. discretize
            toLocation = grid.discretizeMapCoords(toLocation);
            return toLocation;

        }
        /// <summary>
        /// Moves the building to the mouse position
        /// </summary>
        private void relocate()
        {

            Vector3 newDestination = GetNewDestination();
            // if is not a vaild point, the building remains quiet
            if (newDestination == _inputs.invalidPosition) return;

            // 2. check and move alter the color if is not a valid location
            _newBuilding.ghost.transform.position = newDestination;
            if (checkLocation(newDestination))
            {
                _currentPlace = Place.ABLE;
                _newBuilding.material.color = green;
            }
            else
            {
                _currentPlace = Place.NOT_ABLE;
                _newBuilding.material.color = red;
            }

        }
        
    }
}
