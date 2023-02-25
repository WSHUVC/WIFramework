using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIFramework
{
    [Serializable]
    public class Container<T> where T : Component
    {
        public List<T> values = new List<T>();

        public void Loading(IEnumerable<Component> loads)
        {
            foreach (var l in loads)
            {
                if (l is T t)
                    values.Add(t);
            }
        }
    }
}