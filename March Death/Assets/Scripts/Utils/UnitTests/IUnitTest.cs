using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{

    interface IUnitTest
    {
        TestEnvironment testEnvironment { get; set; }
        List<Tuple<string, string>> errorLogger { get; set; }
        string name { get; }

        void Run(float deltaTime);
        void CheckDone();
    }
}
