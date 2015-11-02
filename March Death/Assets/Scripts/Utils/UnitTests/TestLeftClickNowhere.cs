using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils.UnitTests
{
    class TestLeftClickNowhere : UnitTest
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

            // Click nowhere
            topLeftField.SetValue(uinput, new Vector3(0, 0, 0));
            leftClickMethod.Invoke(uinput, null);
        }
    }
}
