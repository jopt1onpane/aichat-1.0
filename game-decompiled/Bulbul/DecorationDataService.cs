using System;
using System.Collections.Generic;
using System.Linq;
using FastEnumUtility;
using R3;
using VContainer;

namespace Bulbul;

public class DecorationDataService : IPresetDataService, IDisposable
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private DecorationService _decorationService;

	private SaveDataManager SaveData => SaveDataManager.Instance;

	public void Migrate()
	{
		DecorationsData decorationSaveData = SaveDataManager.Instance.DecorationSaveData;
		MigrateData(decorationSaveData);
		DecorationsData[] presets = SaveData.DecorationPresetsData.Presets;
		foreach (DecorationsData decorationsData in presets)
		{
			if (decorationsData != null)
			{
				MigrateData(decorationsData);
			}
		}
	}

	private void MigrateData(DecorationsData set)
	{
		DecorationSkinMasterData[] decorationSkins = _masterDataLoader.DecorationMaster.DecorationSkins;
		foreach (DecorationSkinMasterData decorationSkinMasterData in decorationSkins)
		{
			DecorationService.DecorationSkinType skinType = decorationSkinMasterData.SkinType;
			if (set.DecorationDic.ContainsKey(skinType))
			{
				continue;
			}
			set.DecorationDic.Add(skinType, new DecorationData(skinType));
			if (decorationSkinMasterData.IsCategoryDefault)
			{
				DecorationService.DecorationCategoryType category = _masterDataLoader.DecorationMaster.GetCategoryByModel(decorationSkinMasterData.ModelType).CategoryType;
				if (!set.DecorationDic.Where((KeyValuePair<DecorationService.DecorationSkinType, DecorationData> x) => _masterDataLoader.DecorationMaster.GetCategoryBySkin(x.Value.DecorationType).CategoryType == category).Any((KeyValuePair<DecorationService.DecorationSkinType, DecorationData> x) => x.Value.IsActive.Value))
				{
					set.DecorationDic[skinType].IsActive.Value = true;
				}
			}
		}
	}

	public ReadOnlyReactiveProperty<bool> IsDecorationActive(DecorationService.DecorationSkinType skinType)
	{
		return SaveData.DecorationSaveData.DecorationDic[skinType].IsActive;
	}

	public void SetDecorationActive(DecorationService.DecorationSkinType skinType, bool active)
	{
		DecorationData decorationData = SaveData.DecorationSaveData.DecorationDic[skinType];
		if (decorationData.IsActive.Value != active)
		{
			decorationData.IsActive.Value = active;
			SaveData.SaveDecorationThrottled();
		}
	}

	public void LoadPreset(int index)
	{
		DecorationsData orCreatePreset = GetOrCreatePreset(index);
		SaveData.DecorationSaveData.CopyFrom(orCreatePreset);
		SaveData.SaveDecorationThrottled();
		if (SaveData.DecorationPresetsData.SelectedIndex != index)
		{
			SaveData.DecorationPresetsData.SelectedIndex = index;
			SaveData.SaveDecorationPresetThrottled();
		}
		_decorationService?.ReplaceCatEarHeadphoneIfChristmasEvent();
		_decorationService?.ReplaceGlassesIfAprilFoolEvent();
	}

	public void SaveCurrentToPreset(int index, bool alsoSetSelectedIndex)
	{
		GetOrCreatePreset(index).CopyFrom(SaveData.DecorationSaveData);
		if (alsoSetSelectedIndex)
		{
			SaveData.DecorationPresetsData.SelectedIndex = index;
		}
		SaveData.SaveDecorationPresetThrottled();
	}

	public int GetCurrentPresetIndex()
	{
		return SaveData.DecorationPresetsData.SelectedIndex;
	}

	public bool HasDifferenceFromPreset(int index)
	{
		return !SaveData.DecorationSaveData.IsSame(GetOrCreatePreset(index));
	}

	private DecorationsData GetOrCreatePreset(int index)
	{
		DecorationsData decorationsData = SaveData.DecorationPresetsData.Presets[index];
		if (decorationsData == null)
		{
			decorationsData = CreateDefaultData();
			SaveData.DecorationPresetsData.Presets[index] = decorationsData;
		}
		return decorationsData;
	}

	private DecorationsData CreateDefaultData()
	{
		DecorationsData decorationsData = new DecorationsData();
		foreach (DecorationService.DecorationSkinType value in FastEnum.GetValues<DecorationService.DecorationSkinType>())
		{
			DecorationData decorationData = new DecorationData(value);
			decorationsData.DecorationDic.Add(value, decorationData);
			if (_masterDataLoader.DecorationMaster.GetSkin(value).IsCategoryDefault)
			{
				decorationData.IsActive.Value = true;
			}
		}
		return decorationsData;
	}

	public void Dispose()
	{
	}
}
