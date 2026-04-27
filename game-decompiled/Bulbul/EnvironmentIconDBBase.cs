using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

public abstract class EnvironmentIconDBBase : ScriptableObject
{
	protected abstract EnvironmentIconEntry GetBaseEntry(EnvironmentType environmentType);

	public Sprite GetShopThumbnail(EnvironmentType environmentType)
	{
		return GetBaseEntry(environmentType)?.ShopThumbnail;
	}
}
public class EnvironmentIconDBBase<T> : EnvironmentIconDBBase where T : EnvironmentIconEntry
{
	[SerializeField]
	private SerializedDictionaryEnum<EnvironmentType, T> _entries;

	protected T GetEntry(EnvironmentType environmentType)
	{
		return _entries.GetValueOrDefault(environmentType);
	}

	protected sealed override EnvironmentIconEntry GetBaseEntry(EnvironmentType environmentType)
	{
		return GetEntry(environmentType);
	}
}
