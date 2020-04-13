using System;
using System.Collections.Concurrent;

namespace VoxelEngine {
    public class Pool<T>
    {
        private ConcurrentBag<T> _objects;
        private Func<T> _objectGenerator;

        public Pool(Func<T> objectGenerator)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        public T GetObject()
        {
            return _objects.TryTake(out var item) ? item : _objectGenerator();
        }

        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}