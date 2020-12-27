using System;
using System.Collections.Generic;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    [Serializable]
    internal class CustomPropertyList
    {
        public const string kListFieldName = "_list";
    }

    [Serializable]
    internal class CustomPropertyArray<T> : CustomPropertyList
    {
        [SerializeField]
        protected T[] _list = new T[0];

        public T[] list => _list;

        public void CopyFrom(CustomPropertyArray<T> other)
        {
            var otherList = other.list;
            int count = otherList.Length;
            _list = new T[count];
            other.list.CopyTo(_list, 0);
        }

        public void CopyFrom(T[] other)
        {
            int count = other.Length;
            _list = new T[count];
            other.CopyTo(_list, 0);
        }
    }

    [Serializable]
    internal class CustomPropertyList<T> : CustomPropertyList
    {
        [SerializeField]
        protected List<T> _list = new List<T>();

        public List<T> list => _list;
        public T[] array => _list.ToArray();

        public void Add(T item) => _list.Add(item);
        public bool Remove(T item) => _list.Remove(item);
        public bool Contains(T item) => _list.IndexOf(item) > -1;
    }
}