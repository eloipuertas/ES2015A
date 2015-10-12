using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public class RegisterResult<T> : Tuple<T, Action<Object>> where T : struct, IConvertible
    {
        public RegisterResult(T action, Action<Object> func) : base(action, func) { }
    }

    public interface IActor<T> where T : struct, IConvertible
    {
        RegisterResult<T> register(T action, Action<Object> func);
        void unregister(T action, Action<Object> func, bool skipAutoUnregister = false);
    }
}
