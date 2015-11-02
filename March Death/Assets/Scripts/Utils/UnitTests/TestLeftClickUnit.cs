using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils.UnitTests
{
    class TestLeftClickUnit : UnitTest
    {
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

            // Check it is really selected
            if (!unit.getGameObject().GetComponent<Selectable>().currentlySelected)
            {
                LogError("Could not select Unit", unit.ToString());
            }
        }
    }
}
