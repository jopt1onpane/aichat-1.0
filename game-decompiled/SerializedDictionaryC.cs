using System;

[Serializable]
public class SerializedDictionaryC<K, V> : SerializedDictionary<K, V> where K : new()
{
	public override K DefaultKey => new K();
}
