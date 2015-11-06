using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils.UnitTests
{
    class TestLeftClickBuilding : UnitTest
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
            FieldInfo mouseButtonCurrentPointField = uinput.GetType().GetField("mouseButtonCurrentPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo leftClickMethod = uinput.GetType().GetMethod("LeftClick", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo rightClickMethod = uinput.GetType().GetMethod("RightClick", BindingFlags.NonPublic | BindingFlags.Instance);

            IGameEntity building = null;
            foreach (var entity in BasePlayer.player.activeEntities)
            {
                if (entity.info.isBuilding)
                {
                    building = entity;
                    break;
                }
            }

            // Click to unit
            Vector3 point = Camera.main.WorldToScreenPoint(building.getTransform().position);
            mouseButtonCurrentPointField.SetValue(uinput, new Vector2(point.x, point.y));
            leftClickMethod.Invoke(uinput, null);

            // Check it is really selected
            if (!building.getGameObject().GetComponent<Selectable>().currentlySelected)
            {
                LogError("Could not select Building", building.ToString());
            }
        }
    }
}
