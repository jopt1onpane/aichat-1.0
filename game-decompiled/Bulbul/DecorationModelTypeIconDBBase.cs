using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

public abstract class DecorationModelTypeIconDBBase : ScriptableObject
{
	protected abstract DecorationModelTypeIconEntry GetBaseEntry(DecorationService.DecorationModelType decorationModelType);

	public (Sprite, Sprite) GetModelTypeIcon(DecorationService.DecorationModelType decorationModelType)
	{
		DecorationModelTypeIconEntry baseEntry = GetBaseEntry(decorationModelType);
		return (baseEntry?.BaseIcon, baseEntry?.ActiveIcon);
	}
}
public class DecorationModelTypeIconDBBase<T> : DecorationModelTypeIconDBBase where T : DecorationModelTypeIconEntry
{
	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationModelType, T> _entries;

	protected T GetEntry(DecorationService.DecorationModelType decorationModelType)
	{
		return _entries.GetValueOrDefault(decorationModelType);
	}

	protected override DecorationModelTypeIconEntry GetBaseEntry(DecorationService.DecorationModelType decorationModelType)
	{
		return GetEntry(decorationModelType);
	}
}
