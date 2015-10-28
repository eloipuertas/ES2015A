using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    interface IUnitTest
    {
        List<Tuple<string, string>> errorLogger { get; set; };

        void LogError(string title, string message);
        string name { get; }
        void run();
    }
}
