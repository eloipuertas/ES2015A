using UnityEngine;
using System.Collections;
namespace Managers
{
    public class UnitsManager
    {

        private Player _player;
        private UserInput _inputs;
        private ArrayList _selectedUnits { get { return _player.getSelectedObjects(); } }

        // Use this for initialization
        public  UnitsManager( Player player )
        {
            _player = player;
            _inputs = _player.GetComponent<UserInput>();

        }

        public void MoveTo(Vector3 point)
        {
            foreach (Selectable unit in _selectedUnits)
            {
                if (unit.GetComponent<IGameEntity>().info.isUnit)
                    unit.GetComponent<Unit>().moveTo(point);
            }
            Debug.Log("Moving there");
        }


        public void AttackTo(IGameEntity enemy)
        {
            foreach (Selectable unit in _selectedUnits)
            {
                //TODO :(hermetico) check attack buildings too
                if (unit.GetComponent<IGameEntity>().info.isUnit && enemy.info.isUnit)
                {
                    // so far we only can attack units
                    unit.GetComponent<Unit>().attackTarget((Unit)enemy);
                }
            }
            Debug.Log("attacking");
        }
    }
}
