using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    class TestPlayerEntitiesNum : UnitTest
    {
        public string name
        {
            get
            {
                return "Test Player entities == 2";
            }
        }

        public void run(List<Tuple<string, string>> errorLogs)
        {
            if (BasePlayer.player.activeEntities.Count != 2)
            {
                LogError("Player has more than 2 initial entities", BasePlayer.player.currentUnits.Count.ToString());
            }
        }
    }
}
