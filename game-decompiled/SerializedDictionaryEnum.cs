using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SerializedDictionaryEnum<K, V> : SerializedDictionary<K, V> where K : Enum
{
	public override K DefaultKey
	{
		get
		{
			List<K> list = Enum.GetValues(typeof(K)).OfType<K>().ToList();
			if (m_list == null || m_list.Count == 0)
			{
				return list.FirstOrDefault((K k) => !ContainsKey(k));
			}
			List<KeyValue> list2 = m_list;
			for (int num = list.IndexOf(list2[list2.Count - 1].Key) + 1; num < list.Count; num++)
			{
				if (!ContainsKey(list[num]))
				{
					return list[num];
				}
			}
			return list.FirstOrDefault((K k) => !ContainsKey(k));
		}
	}
}
