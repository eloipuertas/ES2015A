using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class AutoUnregister<T> where T : struct, IConvertible
    {
        private IObserver<T> observer;
        private List<RegisterResult<T>> results = new List<RegisterResult<T>>();

        private AutoUnregister(IObserver<T> observer)
        {
            this.observer = observer;
            this.observer.register(this);
        }

        public static implicit operator AutoUnregister<T>(Actor<T> observer)
        {
            return new AutoUnregister<T>(observer);
        }

        public static AutoUnregister<T> operator+ (AutoUnregister<T> self, RegisterResult<T> result)
        {
            self.results.Add(result);
            return self;
        }

        public void unregister(IActor<T> actor, T action, Action<Object> func)
        {
            var key = new RegisterResult<T>(actor, action, func);
            if (results.Contains(key))
            {
                results.Remove(key);
            }
        }

        public void unregisterAll()
        {
            foreach (RegisterResult<T> triplet in results)
            {
                triplet.Key0.unregister(triplet.Key1, triplet.Key2, true);
            }

            observer.unregister(this);
        }
    }
}
