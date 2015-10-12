using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public interface IActor<T> where T : struct, IConvertible
    {
        void register(T action, Action<Object> func);
        void unregister(T action, Action<Object> func);
    }
}
