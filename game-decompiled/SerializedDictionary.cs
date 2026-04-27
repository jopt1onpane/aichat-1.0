using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
{
	[Serializable]
	protected class KeyValue
	{
		public K Key;

		public V Value;

		public KeyValue(K key, V value)
		{
			Key = key;
			Value = value;
		}
	}

	[SerializeField]
	protected List<KeyValue> m_list;

	public virtual K DefaultKey => default(K);

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		Clear();
		foreach (KeyValue item in m_list)
		{
			base[ContainsKey(item.Key) ? DefaultKey : item.Key] = item.Value;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_list = new List<KeyValue>(base.Count);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<K, V> current = enumerator.Current;
			m_list.Add(new KeyValue(current.Key, current.Value));
		}
	}
}
[Serializable]
public class SerializedDictionary<V> : SerializedDictionary<string, V>
{
	public override string DefaultKey => string.Empty;
}
