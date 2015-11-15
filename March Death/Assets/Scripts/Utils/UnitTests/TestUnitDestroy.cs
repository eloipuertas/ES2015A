using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils.UnitTests
{
    class TestUnitDestroy : UnitTest
    {
        public override string name
        {
            get
            {
                return "Test Unit Destroy";
            }
        }

        public override TestEnvironment.States RunAt
        {
            get
            {
                return TestEnvironment.States.POST_GAME_CHECKING;
            }
        }

        IGameEntity entity = null;
        float elapsed = 0;

        public override void Run(float deltaTime)
        {
            if (State == ExecutionState.NOT_STARTED)
            {
                var position = GameObject.Find("PlayerStronghold").transform.position;
                var rotation = Quaternion.Euler(0, 0, 0);
                GameObject gob = Storage.Info.get.createUnit(testEnvironment.playerRace, Storage.UnitTypes.HERO, position, rotation);
                entity = gob.GetComponent<IGameEntity>();

                State = ExecutionState.NOT_DONE;
            }
            else if (State == ExecutionState.NOT_DONE && elapsed > 1)
            {
                entity.Destroy(true);
                State = ExecutionState.DONE;
            }

            elapsed += deltaTime;
        }

        public override void CheckDone()
        {
        }
    }
}
