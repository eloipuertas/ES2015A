using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    class TestPlayerEntitiesNum : UnitTest
    {
        public override string name
        {
            get
            {
                return "Test Player entities == 2";
            }
        }

        public override TestEnvironment.States RunAt
        {
            get
            {
                return TestEnvironment.States.PRE_GAME_CHECKING;
            }
        }

        public override void Run(float deltaTime)
        {
            if (BasePlayer.player.activeEntities.Count != 2)
            {
                LogError("Player has more than 2 initial entities", BasePlayer.player.currentUnits.Count.ToString());
            }
        }
    }
}
