using UnityEngine;
using System.Collections;
using Utils;
namespace Managers
{
    public class BuildingsManager : MonoBehaviour
    {

        Player player;
        UserInput inputs;
        CursorManager cursor;
        private ConstructionGrid grid;
        private bool _placing = false;
        private GameObject newBuilding;
        bool move = true;
        float yoffset = 1f;

        // Use this for initialization
        void Start()
        {
            player = GetComponent<Player>();
            inputs = GetComponent<UserInput>();
            grid = GetComponent<ConstructionGrid>();
            cursor = CursorManager.Instance;

        }

        // Update is called once per frame
        void Update()
        {

            //TODO : (hermetico) test and remove after merging
            #region testcreatebuilding
            bool TEST = false;
            if (TEST) { TEST = false; _createBuilding_("Men-Sawmill"); }
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
                newBuilding = (GameObject)Resources.Load("Prefabs/Buildings/Resources/" + name, typeof(GameObject));
                newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
                this.createBuilding(newBuilding);

            }

        }

        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void createBuilding(Storage.Races race, Storage.BuildingTypes type )
        {
            if (!_placing)
            {
                GameObject newBuilding;
                newBuilding = Storage.Info.get.createBuilding(race, type);
                this.createBuilding(newBuilding);
                PackToMoveBuilding();

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

                this.newBuilding.GetComponent<Rigidbody>().detectCollisions = false;
                _placing = true;
                player.setCurrently(Player.status.PLACING_BUILDING);
            }
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
            if (newDestination == inputs.invalidPosition) return false;

            // alter the color if is not a valid location
            if (checkLocation(newDestination))
            {

                //TODO : (hermetico) restar recursos necesarios para crear el building
                grid.reservePosition(newDestination);
                newBuilding.transform.position = newDestination;
                IGameEntity destination = (IGameEntity)newBuilding.GetComponent<Unit>();
                player.addEntity(destination);
                PackToPlaceBuilding();

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
            //Cursor.visible = true;
            _placing = false;

        }


        private Vector3 GetNewDestination()
        {
            // 1. getPoint
            Vector3 toLocation = inputs.FindTerrainHitPoint();
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
            if (newDestination == inputs.invalidPosition) return;

            // 2. check and move alter the color if is not a valid location
            newBuilding.transform.position = newDestination;
            if (checkLocation(newDestination))
            {
                _drawState(Color.green);
                cursor.setCursor(CursorManager.cursor.DEFAULT);
            }
            else
            {
                _drawState(Color.red);
                cursor.setCursor(CursorManager.cursor.NO_BUILDING_IN);
            }

        }


        private void PackToMoveBuilding()
        {
            if (this.newBuilding)
            { //FIXME : (hermetico) randome errors
                FOWEntity fw = this.newBuilding.GetComponent<FOWEntity>();
                Rigidbody rb = this.newBuilding.GetComponent<Rigidbody>();
                if (fw && rb)
                {
                    fw.IsRevealer = false;
                    rb.detectCollisions = false;
                }
            }


        }

        private void PackToPlaceBuilding()
        {
            this.newBuilding.GetComponent<FOWEntity>().IsRevealer = true;
            this.newBuilding.GetComponent<Rigidbody>().detectCollisions = true;
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

        
        /// <summary>
        /// Moves the location based on the collider, because if we move the object on with the center on the hitpoint
        /// the next hitPoint will be the same object that we are placing, and not the surface of the terrain
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 locationOffset(Vector3 location) {
            Bounds newBounds = newBuilding.GetComponent<Collider>().bounds;
            Vector3 min = newBounds.min;
            Vector3 max = newBounds.max;
            // estos calculos dependen de la camara principal
            // movemos el punto a partir del tamaño del objeto
            // para no situar el objeto encima del raton
            location.x += ((max.x - min.x) / 2f);
            location.z -= ((max.z - min.z) / 2f);
            location.y += ((max.y - min.y) / 2f);

            // ahora hacemos un raicasting, para que el objeto no quede entre
            // del terreno
            return location;// inputs.FindHitPoint(location);
        }
        
    }
}
