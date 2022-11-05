using System;
using System.Collections;

namespace RSToolKit.Domain.Data
{
    public interface ITrail<T>
        : ITrail
        where T : class, ITrailItem
    {
        System.Collections.Generic.IEnumerator<T> GetEnumerator();
        T Pop();
        void Push(T item);
    }

    public interface ITrail
    {
        int Count { get; }
        ITrailItem Pop();
        void Push(ITrailItem item);
    }
}
