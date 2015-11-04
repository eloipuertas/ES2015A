using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils.UnitTests
{
    class TestRightClickMoveUnit : UnitTest
    {
        private float elapsed = 0;

        public override string name
        {
            get
            {
                return "Test Mouse Click";
            }
        }

        public override TestEnvironment.States RunAt
        {
            get
            {
                return TestEnvironment.States.IN_GAME;
            }
        }

        public override void Run(float deltaTime)
        {
            if (State == ExecutionState.NOT_STARTED)
            {
                // Get methods and fields
                UserInput uinput = GameObject.Find("GameController").GetComponent<UserInput>();
                FieldInfo topLeftField = uinput.GetType().GetField("topLeft", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo leftClickMethod = uinput.GetType().GetMethod("LeftClick", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo rightClickMethod = uinput.GetType().GetMethod("RightClick", BindingFlags.NonPublic | BindingFlags.Instance);

                IGameEntity unit = null;
                foreach (var entity in BasePlayer.player.activeEntities)
                {
                    if (entity.info.isUnit)
                    {
                        unit = entity;
                        break;
                    }
                }

                // Click to unit
                topLeftField.SetValue(uinput, Camera.main.WorldToScreenPoint(unit.getTransform().position));
                leftClickMethod.Invoke(uinput, null);

                // Move unit
                topLeftField.SetValue(uinput, new Vector3(0, 0, 0));
                rightClickMethod.Invoke(uinput, null);

                State = ExecutionState.NOT_DONE;
            }

            elapsed += deltaTime;
        }

        public override void CheckDone()
        {
            if (elapsed >= 5)
            {
                State = ExecutionState.DONE;
            }
        }
    }
}
