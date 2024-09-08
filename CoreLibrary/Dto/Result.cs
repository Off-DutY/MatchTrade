using System.Diagnostics.Contracts;

namespace CoreLibrary.Dto
{
    internal enum ResultState : byte
    {
        Faulted,
        Success,
    }

    public readonly struct Result<T>
    {
        private readonly ResultState _state;
        private readonly T _value;
        private readonly Exception _exception;

        public T Value => _value ?? default(T);
        public Exception Exception => _exception ?? new NullExceptionResultException();

        /// <summary>
        /// Constructor of a not valid call
        /// </summary>
        [Obsolete("Don't use this constructor", true)]
        public Result()
        {
            _state = ResultState.Faulted;
            _exception = new NotImplementedException();
            _value = default(T);
        }

        /// <summary>Constructor of a concrete value</summary>
        /// <param name="value"></param>
        public Result(T value)
        {
            _state = ResultState.Success;
            _value = value;
            _exception = null;
        }

        /// <summary>Constructor of an error value</summary>
        /// <param name="e"></param>
        public Result(Exception e)
        {
            _state = ResultState.Faulted;
            _exception = e;
            _value = default(T);
        }

        [Pure]
        public static implicit operator Result<T>(T value) => new Result<T>(value);

        [Pure]
        public static implicit operator Result<T>(Exception ex) => new Result<T>(ex);

        /// <summary>True if the result is faulted</summary>
        [Pure]
        public bool IsFaulted => _state == ResultState.Faulted;

        /// <summary>True if the struct is in an success</summary>
        [Pure]
        public bool IsSuccess => _state == ResultState.Success;

        /// <summary>Convert the value to a showable string</summary>
        [Pure]
        public override string ToString()
        {
            if (IsFaulted)
                return _exception?.ToString() ?? "(null)";
            var a = _value;
            ref var local = ref a;
            return (local != null
                    ? local.ToString()
                    : null) ?? "(null)";
        }

        [Pure]
        public R Match<R>(Func<T, R> succ, Func<Exception, R> fail) => !IsFaulted
                ? succ(_value)
                : fail(Exception);

        [Pure]
        public Result<R> Map<R>(Func<T, R> f) => !IsFaulted
                ? new Result<R>(f(_value))
                : new Result<R>(Exception);
    }
}