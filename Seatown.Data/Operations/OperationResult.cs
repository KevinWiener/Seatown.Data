using System;
using System.Collections.Generic;

namespace Seatown.Data.Operations
{

    public class OperationResult 
    {
        public bool Failure => !Success;
        public bool Success { get; }

        public OperationResult(bool success)
        {
            Success = success;
        }

        public static implicit operator OperationResult(bool result) => new OperationResult(result);
        public static implicit operator bool(OperationResult operation) => operation.Success;
    }

    public class OperationResult<E>
    {
        public E Error { get; }
        public bool Failure => !Success;
        public bool Success { get; }

        public OperationResult(bool success)
        {
            Success = success;
        }

        public OperationResult(E error)
        {
            Error = error;
            Success = false;
        }

        public static implicit operator OperationResult<E>(bool result) => new OperationResult<E>(result);
        public static implicit operator bool(OperationResult<E> operation) => operation.Success;
        public static implicit operator OperationResult<E>(E error) => new OperationResult<E>(error);
        public static implicit operator E(OperationResult<E> operation) => operation.Error;
    }

    public class OperationResult<R, E> 
    {
        public E Error { get; }
        public bool Failure => !Success;
        public R Result { get; }
        public bool Success { get; }

        public OperationResult(R result)
        {
            Result = result;
            Success = true;
        }

        public OperationResult(E error)
        {
            Error = error;
            Success = false;
        }

        public static implicit operator bool(OperationResult<R, E> operation) => operation.Success;
        public static implicit operator OperationResult<R, E>(R result) => new OperationResult<R, E>(result);
        public static implicit operator OperationResult<R, E>(E error) => new OperationResult<R, E>(error);
        public static implicit operator R(OperationResult<R, E> operation) => operation.Result;
        public static implicit operator E(OperationResult<R, E> operation) => operation.Error;
    }

}
