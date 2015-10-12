using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class AutoUnregister<T> where T : struct, IConvertible
    {
        private Actor<T> actor;
        private List<Tuple<T, Action<Object>>> results = new List<Tuple<T, Action<Object>>>();

        private AutoUnregister(Actor<T> actor)
        {
            this.actor = actor;
            actor.register(this);
        }

        public static implicit operator AutoUnregister<T>(Actor<T> actor)
        {
            return new AutoUnregister<T>(actor);
        }

        public static AutoUnregister<T> operator+ (AutoUnregister<T> self, Tuple<T, Action<Object>> result)
        {
            self.results.Add(result);
            return self;
        }

        public void unregister(T action, Action<Object> func)
        {
            var key = new RegisterResult<T>(action, func);
            if (results.Contains(key))
            {
                results.Remove(key);
                actor.unregister(action, func, true);
            }

            if (results.Count == 0)
            {
                actor.unregister(this);
            }
        }

        public void unregisterAll()
        {
            foreach (RegisterResult<T> pair in results)
            {
                actor.unregister(pair.Key0, pair.Key1);
            }

            actor.unregister(this);
        }
    }
}
