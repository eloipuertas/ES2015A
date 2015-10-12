using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public interface IObserver<T> where T : struct, IConvertible
    {
        void OnDestroy();
        void register(AutoUnregister<T> auto);
        void unregister(AutoUnregister<T> auto);
    }
}
