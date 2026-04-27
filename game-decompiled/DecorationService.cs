using System.Collections.Generic;
using System.Linq;
using Bulbul;
using FastEnumUtility;
using GUPS.Obfuscator.Attribute;
using R3;
using UnityEngine;
using VContainer;

public class DecorationService : MonoBehaviour
{
	public enum DecorationCategoryType
	{
		Cup = 1,
		Headphone,
		Glasses,
		BookLayout,
		Book,
		Keyboard,
		StandLight,
		Desk,
		Chair,
		CoffeeMaker,
		Badge
	}

	public enum DecorationModelType
	{
		Cup_1 = 1001,
		Cup_2 = 1002,
		Cup_3 = 1003,
		Cup_4_BearsRestaurant = 1004,
		Headphone_1 = 2001,
		Headphone_2 = 2002,
		Glasses_1 = 3001,
		Glasses_2 = 3002,
		Glasses_3 = 3003,
		Glasses_None = 3999,
		BookLayout = 4001,
		Book_1_BearsRestaurant = 5001,
		Keyboard_1 = 6001,
		Keyboard_2 = 6002,
		Keyboard_3 = 6003,
		StandLight_1 = 7001,
		StandLight_2 = 7002,
		StandLight_3 = 7003,
		Desk_1 = 8001,
		Desk_2 = 8002,
		Desk_3 = 8003,
		Chair_1 = 9001,
		Chair_2 = 9002,
		Chair_3 = 9003,
		CoffeeMaker_1 = 10001,
		Badge_1 = 11001
	}

	[DoNotRename]
	public enum DecorationSkinType
	{
		MugCup_1 = 0,
		MugCup_2 = 1,
		MugCup_3 = 2,
		Cup_2A = 1002001,
		Cup_2B = 1002002,
		Cup_2C = 1002003,
		Cup_2D = 1002004,
		Cup_3A = 1003001,
		Cup_3B = 1003002,
		Cup_3C = 1003003,
		Cup_3D = 1003004,
		Cup_4A_BearsRestaurant = 1004001,
		Headphone_1 = 3,
		Headphone_1B = 2001002,
		Headphone_1C = 2001003,
		Headphone_1D = 2001004,
		Headphone_2 = 4,
		Headphone_2B = 2002002,
		Headphone_2C = 2002003,
		Headphone_2D = 2002004,
		Grasses_1 = 5,
		Glasses_1B = 3001002,
		Glasses_1C = 3001003,
		Glasses_1D = 3001004,
		Glasses_2A = 3002001,
		Glasses_2B = 3002002,
		Glasses_2C = 3002003,
		Glasses_2D = 3002004,
		Glasses_3A = 3003001,
		Glasses_3B = 3003002,
		Glasses_3C = 3003003,
		Glasses_3D = 3003004,
		Grasses_2 = 6,
		Book_1 = 7,
		Book_2 = 8,
		Book_3 = 9,
		Book_4 = 10,
		Book_5 = 11,
		Book_1A_BearsRestaurant = 5001000,
		Keyboard_1A = 6001001,
		Keyboard_1B = 6001002,
		Keyboard_1C = 6001003,
		Keyboard_1D = 6001004,
		Keyboard_2A = 6002001,
		Keyboard_2B = 6002002,
		Keyboard_2C = 6002003,
		Keyboard_2D = 6002004,
		Keyboard_3A = 6003001,
		Keyboard_3B = 6003002,
		Keyboard_3C = 6003003,
		Keyboard_3D = 6003004,
		StandLight_1A = 7001001,
		StandLight_1B = 7001002,
		StandLight_1C = 7001003,
		StandLight_1D = 7001004,
		StandLight_2A = 7002001,
		StandLight_2B = 7002002,
		StandLight_2C = 7002003,
		StandLight_2D = 7002004,
		StandLight_3A = 7003001,
		StandLight_3B = 7003002,
		StandLight_3C = 7003003,
		StandLight_3D = 7003004,
		Desk_1A = 8001001,
		Desk_1B = 8001002,
		Desk_1C = 8001003,
		Desk_1D = 8001004,
		Desk_2A = 8002001,
		Desk_2B = 8002002,
		Desk_2C = 8002003,
		Desk_2D = 8002004,
		Desk_3A = 8003001,
		Desk_3B = 8003002,
		Desk_3C = 8003003,
		Desk_3D = 8003004,
		Chair_1A = 9001001,
		Chair_1B = 9001002,
		Chair_1C = 9001003,
		Chair_1D = 9001004,
		Chair_2A = 9002001,
		Chair_2B = 9002002,
		Chair_2C = 9002003,
		Chair_2D = 9002004,
		Chair_3A = 9003001,
		Chair_3B = 9003002,
		Chair_3C = 9003003,
		Chair_3D = 9003004,
		CoffeeMaker_1A = 10001001,
		CoffeeMaker_1B = 10001002,
		CoffeeMaker_1C = 10001003,
		CoffeeMaker_1D = 10001004,
		Badge_1A = 11001001,
		Badge_1B = 11001002
	}

	private struct DecorationData
	{
		public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>();

		public DecorationData(bool isActive)
		{
			_isActive.Value = isActive;
		}
	}

	[Inject]
	private MasterDataLoader _masterDataLoader;

	private Dictionary<DecorationCategoryType, List<DecorationModelChanger>> _modelViewDic;

	private ReactiveProperty<DecorationModelType> _currentMugCupModel = new ReactiveProperty<DecorationModelType>();

	private ReactiveProperty<DecorationModelType> _currentKeyboardModel = new ReactiveProperty<DecorationModelType>();

	public ReactiveProperty<DecorationModelType> CurrentMugCupModel => _currentMugCupModel;

	public ReactiveProperty<DecorationModelType> CurrentKeyboardModel => _currentKeyboardModel;

	private DecorationsData DecorationSaveData => SaveDataManager.Instance.DecorationSaveData;

	public void Setup()
	{
		_modelViewDic = (from x in Object.FindObjectsByType<DecorationModelChanger>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			group x by x.CategoryType).ToDictionary((IGrouping<DecorationCategoryType, DecorationModelChanger> x) => x.Key, (IGrouping<DecorationCategoryType, DecorationModelChanger> x) => x.ToList());
		ApplyDecorationBySavedata();
		UpdateModelCache();
	}

	public void ApplyDecorationBySavedata()
	{
		foreach (DecorationCategoryType value in FastEnum.GetValues<DecorationCategoryType>())
		{
			DeactivateAllModels(value, isSave: false);
		}
		foreach (var (skin, decorationData2) in SaveDataManager.Instance.DecorationSaveData.DecorationDic)
		{
			if (decorationData2.IsActive.Value)
			{
				ChangeDecoration(skin, isSave: false);
			}
		}
		AdjustApplyDecoration();
	}

	public void DeactivateAllModels(DecorationCategoryType category, bool isSave)
	{
		bool flag = false;
		if (isSave)
		{
			foreach (var (skinType, decorationData2) in DecorationSaveData.DecorationDic)
			{
				if (_masterDataLoader.DecorationMaster.GetModelBySkin(skinType).CategoryType == category)
				{
					flag = decorationData2.IsActive.Value;
					decorationData2.IsActive.Value = false;
				}
			}
		}
		if (_modelViewDic.TryGetValue(category, out var value))
		{
			foreach (DecorationModelChanger item in value)
			{
				item.DeactivateCategory();
			}
		}
		if (flag)
		{
			SaveDataManager.Instance.SaveDecorationThrottled();
		}
	}

	public bool ParseDecorationTypeForString(string type, out DecorationSkinType decorationType)
	{
		if (FastEnum.TryParse<DecorationSkinType>(type, out decorationType))
		{
			return true;
		}
		return false;
	}

	public void ChangeDecoration(DecorationSkinType skin, bool isSave)
	{
		bool flag = false;
		if (isSave)
		{
			flag = ChangeSaveData(skin);
		}
		DecorationModelMasterData modelBySkin = _masterDataLoader.DecorationMaster.GetModelBySkin(skin);
		if (modelBySkin != null && _modelViewDic.TryGetValue(modelBySkin.CategoryType, out var value))
		{
			foreach (DecorationModelChanger item in value)
			{
				item.Apply(modelBySkin.ModelType, skin);
			}
		}
		if (modelBySkin != null)
		{
			if (modelBySkin.CategoryType == DecorationCategoryType.Cup)
			{
				_currentMugCupModel.Value = modelBySkin.ModelType;
			}
			else if (modelBySkin.CategoryType == DecorationCategoryType.Keyboard)
			{
				_currentKeyboardModel.Value = modelBySkin.ModelType;
			}
		}
		if (flag)
		{
			SaveDataManager.Instance.SaveDecorationThrottled();
		}
	}

	private bool ChangeSaveData(DecorationSkinType skinType)
	{
		DecorationCategoryType categoryType = _masterDataLoader.DecorationMaster.GetModelBySkin(skinType).CategoryType;
		IEnumerable<DecorationSkinMasterData> skinsByCategory = _masterDataLoader.DecorationMaster.GetSkinsByCategory(categoryType);
		bool result = false;
		foreach (DecorationSkinMasterData item in skinsByCategory)
		{
			ReactiveProperty<bool> isActive = DecorationSaveData.DecorationDic[item.SkinType].IsActive;
			bool flag = item.SkinType == skinType;
			if (isActive.CurrentValue != flag)
			{
				DecorationSaveData.DecorationDic[item.SkinType].IsActive.Value = flag;
				result = true;
			}
		}
		return result;
	}

	private void UpdateModelCache()
	{
		IEnumerable<DecorationSkinMasterData> skinsByCategory = _masterDataLoader.DecorationMaster.GetSkinsByCategory(DecorationCategoryType.Cup);
		_currentMugCupModel.Value = DecorationModelType.Cup_1;
		foreach (DecorationSkinMasterData item in skinsByCategory)
		{
			if (DecorationSaveData.DecorationDic.TryGetValue(item.SkinType, out var value) && value.IsActive.Value)
			{
				DecorationModelMasterData modelBySkin = _masterDataLoader.DecorationMaster.GetModelBySkin(item.SkinType);
				_currentMugCupModel.Value = modelBySkin.ModelType;
				break;
			}
		}
		IEnumerable<DecorationSkinMasterData> skinsByCategory2 = _masterDataLoader.DecorationMaster.GetSkinsByCategory(DecorationCategoryType.Keyboard);
		_currentKeyboardModel.Value = DecorationModelType.Keyboard_1;
		foreach (DecorationSkinMasterData item2 in skinsByCategory2)
		{
			if (DecorationSaveData.DecorationDic.TryGetValue(item2.SkinType, out var value2) && value2.IsActive.Value)
			{
				DecorationModelMasterData modelBySkin2 = _masterDataLoader.DecorationMaster.GetModelBySkin(item2.SkinType);
				_currentKeyboardModel.Value = modelBySkin2.ModelType;
				break;
			}
		}
	}

	public void AdjustApplyDecoration()
	{
		ReplaceCatEarHeadphoneIfChristmasEvent();
		ReplaceGlassesIfAprilFoolEvent();
	}

	public void ReplaceCatEarHeadphoneIfChristmasEvent()
	{
		if (SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.Christmas2025 && _masterDataLoader.DecorationMaster.GetSkinsByModel(DecorationModelType.Headphone_2).Any((DecorationSkinMasterData skin) => SaveDataManager.Instance.DecorationSaveData.DecorationDic.TryGetValue(skin.SkinType, out var value) && value.IsActive.Value))
		{
			ChangeDecoration(DecorationSkinType.Headphone_1, isSave: false);
		}
	}

	public void ReplaceGlassesIfAprilFoolEvent()
	{
		if (SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.AprilFool2026)
		{
			ChangeDecoration(DecorationSkinType.Grasses_2, isSave: false);
		}
	}
}
