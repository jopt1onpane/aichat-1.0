using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

public abstract class DecorationSkinTypeIconDBBase : ScriptableObject
{
	protected abstract DecorationSkinTypeIconEntry GetBaseEntry(DecorationService.DecorationSkinType decorationSkinType);

	public Sprite GetSkinTypeIcon(DecorationService.DecorationSkinType decorationSkinType)
	{
		return GetBaseEntry(decorationSkinType)?.Icon;
	}
}
public class DecorationSkinTypeIconDBBase<T> : DecorationSkinTypeIconDBBase where T : DecorationSkinTypeIconEntry
{
	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, T> _entries;

	protected T GetEntry(DecorationService.DecorationSkinType decorationSkinType)
	{
		return _entries.GetValueOrDefault(decorationSkinType);
	}

	protected override DecorationSkinTypeIconEntry GetBaseEntry(DecorationService.DecorationSkinType decorationSkinType)
	{
		return GetEntry(decorationSkinType);
	}
}
