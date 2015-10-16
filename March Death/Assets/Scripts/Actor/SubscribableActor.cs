using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    // T, enum actions
    // S, base class
    public abstract class SubscribableActor<T, S> : Actor<T> where T : struct, IConvertible where S : class
    {
        public SubscribableActor() { }

        public override void Start()
        {
            Subscriber<T, S>.get.onActorStart(this);
        }
    }
}
