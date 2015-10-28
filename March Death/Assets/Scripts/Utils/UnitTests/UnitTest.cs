using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    abstract class UnitTest : IUnitTest
    {
        List<Tuple<string, string>> errorLogger { get; set; };

        protected void LogError(string title, string message)
        {
            errorLogger.Add(new Tuple<string, string>(title, message));
        }

        abstract string name { get; }
        abstract void run();
    }
}
