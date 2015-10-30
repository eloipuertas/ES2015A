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

        public override void run()
        {
            if (BasePlayer.player.activeEntities.Count != 2)
            {
                LogError("Player has more than 2 initial entities", BasePlayer.player.currentUnits.Count.ToString());
            }
        }
    }
}
