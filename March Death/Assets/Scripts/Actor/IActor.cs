using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public class RegisterResult<T> : Triplet<IActor<T>, T, Action<Object>> where T : struct, IConvertible
    {
        public RegisterResult (IActor<T> actor, T action, Action<Object> func) :
            base(actor, action, func)
        {
        }
    }

    public interface IActor<T> : IBaseActor where T : struct, IConvertible
    {
        RegisterResult<T> register (T action, Action<Object> func);
    }

    public interface IBaseActor
    {
        IKeyGetter unregister<A> (A action, Action<Object> func);
    }
}
