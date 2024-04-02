using System.Collections.Generic;
using UnityEngine;


namespace WI
{
    [System.Serializable]
    public struct SKeyValuePair<T, T2>
    {
        [SerializeField] public T Key;
        [SerializeField] public T2 Value;
        public SKeyValuePair(T key, T2 value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// SerializeDictionary For Unity
    /// </summary>
    [System.Serializable]
    public partial class SDictionary<T, T2> : Dictionary<T, T2>
    {
        [SerializeField] public List<SKeyValuePair<T, T2>> datas = new List<SKeyValuePair<T, T2>>();

        public SDictionary()
        {
            foreach (var d in datas)
            {
                Add(d.Key, d.Value);
            }
        }

        void Refresh()
        {
            if (base.Count == datas.Count)
                return;
            foreach (var d in datas)
            {
                base.TryAdd(d.Key, d.Value);
            }
        }

        public new T2 this[T key]
        {
            get
            {
                Refresh();
                return base[key];
            }

            set
            {
                for (int i = 0; i < datas.Count; ++i)
                {
                    var currentData = datas[i];
                    var ck = currentData.Key;
                    if (ck.Equals(key))
                        datas.RemoveAt(i);
                }

                datas.Add(new SKeyValuePair<T, T2>(key, value));
                base[key] = value;
            }
        }

        public bool Add(in T key, in T2 value)
        {
            if (base.ContainsKey(key))
                return false;

            base.Add(key, value);
            datas.Add(new SKeyValuePair<T, T2>(key, value));
            return true;
        }

        public bool Remove(in T key)
        {
            if (!base.ContainsKey(key))
                return false;
            for (int i = 0; i < datas.Count; ++i)
            {
                if (datas[i].Key.Equals(key))
                {
                    datas.RemoveAt(i);
                    base.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public new void Clear()
        {
            base.Clear();
            datas.Clear();
        }

        public bool TryGetValue(in T key, out T2 value)
        {
            Refresh();
            return base.TryGetValue(key, out value);
        }
    }
}