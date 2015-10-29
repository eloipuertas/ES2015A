using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    abstract class UnitTest : IUnitTest
    {
        public List<Tuple<string, string>> errorLogger { get; set; }

        protected void LogError(string title, string message)
        {
            errorLogger.Add(new Tuple<string, string>(title, message));
        }

        public abstract string name { get; }
        public abstract void run();
    }
}
