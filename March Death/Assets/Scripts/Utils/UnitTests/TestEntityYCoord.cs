using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    class TestEntityYCoord : UnitTest
    {
        public override string name
        {
            get
            {
                return "Test IGameEntity Y coordinate >= 0";
            }
        }

        public override void run()
        {
            Unit[] entities = UnityEngine.GameObject.FindObjectsOfType<Unit>();
            foreach (Unit unit in entities)
            {
                if (unit.transform.position.y < 0)
                {
                    LogError("Unit Y coordinate < 0", unit.ToString());
                }
            }
        }
    }
}
