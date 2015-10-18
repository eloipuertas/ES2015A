using UnityEngine;
using System.Collections;
namespace Managers
{
    public class BuildingsManager : MonoBehaviour
    {

        Player player;
        UserInput inputs;

        private bool _placing = false;
        private GameObject newBuilding;

        // Use this for initialization
        void Start()
        {
            player = GetComponent<Player>();
            inputs = GetComponent<UserInput>();

        }

        // Update is called once per frame
        void Update()
        {

            //TODO : (hermetico) test and remove after merging
            #region testcreatebuilding
            bool TEST = false;
            if (TEST) { TEST = false; _createBuilding("elf-farm"); }
            #endregion
            if (_placing)
            {
                relocate();

            }

        }

        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void _createBuilding_(string name)
        {
            if (!_placing)
            {
                GameObject newBuilding;
                newBuilding = (GameObject)Resources.Load("Prefabs/Buildings/" + name, typeof(GameObject));
                newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
                this.createBuilding(newBuilding);

            }

        }
        
        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void createBuilding(string path)
        {
            if (!_placing)
            {
                GameObject newBuilding;
                newBuilding = (GameObject)Resources.Load( path, typeof(GameObject));
                newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
                this.createBuilding(newBuilding);

            }

        }

        /// <summary>
        /// Starts creating a building. The param newBuilding must be instantiated previously
        /// </summary>
        /// <param name="newBuilding"></param>
        public void createBuilding(GameObject newBuilding)
        {
            if (!_placing)
            {
                this.newBuilding = newBuilding;
                newBuilding.GetComponent<Collider>().isTrigger = true; //HACK : (hermetico) controlar colision objeto con resto
                _placing = true;
                Cursor.visible = false;
                player.setCurrently(Player.status.PLACING_BUILDING);

            }
        }

        /// <summary>
        /// Discretizes the location through ConstructioGrid
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 adaptLocation(Vector3 location)
        {
            // TODO : (hermetico) uncomment when merge with devel_c
            //location = Utils.ConstructionGrid.discretizeMapCoords(toLocation);
            return location;
        }

        /// <summary>
        /// Checks if is valid locatoin through ConstructorGrid
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool checkLocation(Vector3 location)
        {
            bool check = false;

            // TODO : (hermetico) uncomment when merge with devel_c
            //check = Utils.isNewPositionAbleForConstrucction(location)
            check = location.y < 89 ? true : false;

            return check;

        }

        /// <summary>
        /// Places the building, checking if is a suitable place
        /// </summary>
        public void placeBuilding()
        {
            Vector3 toLocation = inputs.FindHitPoint();
            toLocation = adaptLocation(toLocation);

            // alter the color if is not a valid location
            if (checkLocation(toLocation))
            {
                //TODO : (hermetico) restar recursos necesarios para crear el building
                //Utils.ConstructionGrid.resevePosition(toLocation)
                newBuilding.transform.position = toLocation;
                IGameEntity destination = (IGameEntity)newBuilding.GetComponent<Unit>();
                player.addEntityToList(destination);
                newBuilding.GetComponent<Collider>().isTrigger = false; //HACK : (hermetico) controlar colision objeto con resto
                                                                        // remaining operations
                _finishPlacing();
            }


        }

        /// <summary>
        /// Cancel the placing of the building and returns player to idle status
        /// </summary>
        public void cancelPlacing()
        {
            if (_placing)
            {
                Destroy(newBuilding);
                // remaining operations
                _finishPlacing();
            }
        }

        /// <summary>
        /// Common operations when placing or cancelling placing
        /// </summary>

        private void _finishPlacing()
        {
            Cursor.visible = true;
            _placing = false;
            player.setCurrently(Player.status.IDLE);
        }

        /// <summary>
        /// Moves the building to the mouse position
        /// </summary>
        private void relocate()
        {


            Vector3 toLocation = inputs.FindHitPoint();

            // move the object to match toLocation with the min corner


            toLocation = adaptLocation(toLocation);

            // alter the color if is not a valid location
            if (checkLocation(toLocation))
            {
                _drawState(Color.green);
            }
            else
            {
                _drawState(Color.red);
            }

            newBuilding.transform.position = toLocation;

        }


        /// <summary>
        /// Draws a surrounding box based on the collider
        /// </summary>
        /// <param name="lineColor"></param>
        private void _drawState(Color lineColor)
        {
            Vector3 boundPoint1, boundPoint2, boundPoint3, boundPoint4, boundPoint5, boundPoint6, boundPoint7, boundPoint8;
            Bounds bounds = newBuilding.GetComponent<Collider>().bounds;
            boundPoint1 = bounds.min;
            boundPoint2 = bounds.max;
            boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
            boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
            boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
            boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
            boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
            boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);


            // rectangular cuboid
            // top of rectangular cuboid (6-2-8-4)
            Debug.DrawLine(boundPoint6, boundPoint2, lineColor);
            Debug.DrawLine(boundPoint2, boundPoint8, lineColor);
            Debug.DrawLine(boundPoint8, boundPoint4, lineColor);
            Debug.DrawLine(boundPoint4, boundPoint6, lineColor);

            // bottom of rectangular cuboid (3-7-5-1)
            Debug.DrawLine(boundPoint3, boundPoint7, lineColor);
            Debug.DrawLine(boundPoint7, boundPoint5, lineColor);
            Debug.DrawLine(boundPoint5, boundPoint1, lineColor);
            Debug.DrawLine(boundPoint1, boundPoint3, lineColor);

            // legs (6-3, 2-7, 8-5, 4-1)
            Debug.DrawLine(boundPoint6, boundPoint3, lineColor);
            Debug.DrawLine(boundPoint2, boundPoint7, lineColor);
            Debug.DrawLine(boundPoint8, boundPoint5, lineColor);
            Debug.DrawLine(boundPoint4, boundPoint1, lineColor);

        }

        /*
        /// <summary>
        /// Moves the location based on the collider, because if we move the object on with the center on the hitpoint
        /// the next hitPoint will be the same object that we are placing, and not the surface of the terrain
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 locationTrick(Vector3 location) {
            Vector3 min = newBuilding.GetComponent<Collider>().bounds.min;
            Vector3 max = newBuilding.GetComponent<Collider>().bounds.max;
            location += ((max - min) / 2f);
            return location;
        }
        */
    }
}