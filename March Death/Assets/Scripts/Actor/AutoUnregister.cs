using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class AutoUnregister<ObserverType> where ObserverType : struct, IConvertible
    {
        private IObserver<ObserverType> observer;
        private List<IKeyGetter> results = new List<IKeyGetter>();

        public AutoUnregister(IObserver<ObserverType> observer)
        {
            this.observer = observer;
            this.observer.register(this);
        }

        public static AutoUnregister<ObserverType> operator +(AutoUnregister<ObserverType> self, IKeyGetter result)
        {
            self.results.Add(result);
            return self;
        }

        public void unregister<T>(IActor<T> actor, T action, Action<Object> func) where T : struct, IConvertible
        {
            var key = new RegisterResult<T>(actor, action, func);
            if (results.Contains(key))
            {
                results.Remove(key);
            }
        }

        public void unregisterAll<T>() where T : struct, IConvertible
        {
            foreach (var triplet in results)
            {
                RegisterResult<T> result = triplet as RegisterResult<T>;
                if (result != null)
                {
                    result.Key0.unregister(result.Key1, result.Key2, true);
                }
            }

            observer.unregister(this);
        }
        
        public static implicit operator AutoUnregister<ObserverType>(Actor<ObserverType> observer)
        {
            return new AutoUnregister<ObserverType>(observer);
        }
    }
}
