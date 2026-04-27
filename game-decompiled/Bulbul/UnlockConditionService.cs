using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using FastEnumUtility;
using VContainer;

namespace Bulbul;

public class UnlockConditionService : IDisposable
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PlayerPointService _pointService;

	private SaveDataManager SaveData => SaveDataManager.Instance;

	public (bool unlocked, bool notLockCondition) IsUnlocked<TItemType>(TItemType itemType) where TItemType : struct, Enum
	{
		IUnlockConditionData itemMaster = GetItemMaster(itemType);
		if (itemMaster == null)
		{
			Debug.LogError($"解放要素: {typeof(TItemType).Name}.{itemType}に対するマスタが取得できませんでした。");
			return (unlocked: false, notLockCondition: false);
		}
		UnlockItemService.ConditionsType conditionsType = FastEnum.Parse<UnlockItemService.ConditionsType>(itemMaster.ConditionsType);
		switch (conditionsType)
		{
		case UnlockItemService.ConditionsType.None:
			return (unlocked: true, notLockCondition: true);
		case UnlockItemService.ConditionsType.Scenario:
			if (itemMaster.Arg1 == "None")
			{
				return (unlocked: true, notLockCondition: true);
			}
			return (unlocked: SaveData.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(itemMaster.Arg1), notLockCondition: false);
		case UnlockItemService.ConditionsType.PointPurchase:
			return (unlocked: GetPurchasedList<TItemType>().Contains(itemType.ToName()), notLockCondition: false);
		default:
			Debug.LogError($"無効な条件タイプ: {conditionsType}");
			return (unlocked: false, notLockCondition: false);
		}
	}

	public bool IsPurchasableItem<TItemType>(TItemType itemType, out int price) where TItemType : struct, Enum
	{
		IUnlockConditionData itemMaster = GetItemMaster(itemType);
		if (itemMaster == null)
		{
			price = 0;
			return false;
		}
		if (itemMaster.ConditionsType != UnlockItemService.ConditionsType.PointPurchase.ToName())
		{
			price = 0;
			return false;
		}
		if (GetPurchasedList<TItemType>().Contains(itemType.ToName()))
		{
			price = 0;
			return false;
		}
		if (!int.TryParse(itemMaster.Arg1, out price))
		{
			Debug.LogError($"{itemType}のArg1が整数ではありません。");
			return false;
		}
		return true;
	}

	public bool CanPurchase<TItemType>(TItemType itemType) where TItemType : struct, Enum
	{
		if (IsPurchasableItem(itemType, out var price))
		{
			return _pointService.Point >= price;
		}
		return false;
	}

	public bool Purchase<TItemType>(TItemType itemType) where TItemType : struct, Enum
	{
		if (!IsPurchasableItem(itemType, out var price))
		{
			Debug.LogWarning("購入可能なアイテムではない");
			return false;
		}
		if (!_pointService.ConsumePoint(price, save: false))
		{
			Debug.LogWarning("ポイントが足りない");
			return false;
		}
		GetPurchasedList<TItemType>().Add(itemType.ToName());
		SaveData.SavePointPurchaseData();
		_pointService.SavePoint();
		return true;
	}

	private IUnlockConditionData GetItemMaster<TItemType>(TItemType itemType)
	{
		if (itemType is EnvironmentType)
		{
			EnvironmentType value = (EnvironmentType)((((object)itemType) is EnvironmentType) ? ((object)itemType) : null);
			return _masterDataLoader.UnlockEnvironmentMasterList.FirstOrDefault((UnlockEnvironmentData x) => x.ItemType == value.ToName());
		}
		if (itemType is DecorationService.DecorationSkinType)
		{
			DecorationService.DecorationSkinType value2 = (DecorationService.DecorationSkinType)((((object)itemType) is DecorationService.DecorationSkinType) ? ((object)itemType) : null);
			return _masterDataLoader.UnlockDecorationMasterList.FirstOrDefault((UnlockDecorationData x) => x.ItemType == value2.ToName());
		}
		return null;
	}

	private List<string> GetPurchasedList<TItemType>()
	{
		if (typeof(TItemType) == typeof(EnvironmentType))
		{
			return SaveData.PointPurchaseData.PurchasedEnvironments;
		}
		if (typeof(TItemType) == typeof(DecorationService.DecorationSkinType))
		{
			return SaveData.PointPurchaseData.PurchasedDecorations;
		}
		return null;
	}

	public void Dispose()
	{
	}
}
