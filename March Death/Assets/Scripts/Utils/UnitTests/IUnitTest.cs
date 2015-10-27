using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    interface IUnitTest
    {
        string name { get; }
        void run(List<Tuple<String, String>> errorLogs);
    }
}
