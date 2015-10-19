using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class AutoUnregister
    {
        private IObserver observer;
        private List<IKeyGetter> results = new List<IKeyGetter>();

        public AutoUnregister(IObserver observer)
        {
            this.observer = observer;
            this.observer.register(this);
        }

        public static AutoUnregister operator +(AutoUnregister self, IKeyGetter result)
        {
            self.results.Add(result);
            return self;
        }

        public static AutoUnregister operator -(AutoUnregister self, IKeyGetter result)
        {
            self.results.Remove(result);
            return self;
        }

        public void unregisterAll()
        {
            foreach (var triplet in results)
            {
                IBaseActor actor = (IBaseActor)triplet.getKey(0);
                actor.unregister(triplet.getKey(1), (Action<Object>)triplet.getKey(2));
            }

            observer.unregister(this);
        }
        
        public static implicit operator AutoUnregister(Observer observer)
        {
            return new AutoUnregister(observer);
        }

        public static implicit operator AutoUnregister(UnityEngine.MonoBehaviour observer)
        {
            return new AutoUnregister((IObserver)observer);
        }
    }
}
