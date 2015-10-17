using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public interface IObserver
    {
        void OnDestroy();
        void register(AutoUnregister auto);
        void unregister(AutoUnregister auto);
    }
}
