using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.UnitTests
{
    abstract class UnitTest : IUnitTest
    {
        public enum ExecutionState { NOT_STARTED, NOT_DONE, DONE }

        public TestEnvironment testEnvironment { get; set; }
        public List<Tuple<string, string>> errorLogger { get; set; }
        public ExecutionState State { get; set; }

        public abstract string name { get; }
        public abstract TestEnvironment.States RunAt { get; }

        public UnitTest()
        {
            State = ExecutionState.NOT_STARTED;
        }

        protected void LogError(string title, string message)
        {
            errorLogger.Add(new Tuple<string, string>(title, message));
        }
    
        public abstract void Run(float deltaTime);
        public virtual void CheckDone() { State = ExecutionState.DONE; }
    }
}
