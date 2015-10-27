using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    class TestEntityYCoord : IUnitTest
    {
        public string name
        {
            get
            {
                return "Test IGameEntity Y coordinate >= 0";
            }
        }

        public void run(List<Tuple<string, string>> errorLogs)
        {
            Unit[] entities = UnityEngine.GameObject.FindObjectsOfType<Unit>();
            foreach (Unit unit in entities)
            {
                if (unit.transform.position.y < 0)
                {
                    errorLogs.Add(new Tuple<string, string>("Unit Y coordinate < 0", unit.ToString()));
                }
            }
        }
    }
}
