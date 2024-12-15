using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>,ISerializationCallbackReceiver
{
    [Serializable]
    public class Pair
    {
        public TKey key = default;
        public TValue value = default;

        /// <summary>
        /// Pair
        /// </summary>
        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [SerializeField]
    private List<Pair> _list = new List<Pair>();

    /// <summary>
    /// OnAfterDeserialize
    /// </summary>
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Clear();
        foreach (Pair pair in _list)
        {
            if (ContainsKey(pair.key))
            {
                continue;
            }
            Add(pair.key, pair.value);
        }
    }

    /// <summary>
    /// OnBeforeSerialize
    /// </summary>
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        /*
        _list.Clear();
        foreach (KeyValuePair<TKey, TValue> kvp in this)
        {
            _list.Add(new Pair(kvp.Key, kvp.Value));
        }
        */
    }
}