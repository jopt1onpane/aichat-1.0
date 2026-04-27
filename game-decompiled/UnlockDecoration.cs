using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using FastEnumUtility;
using R3;

public class UnlockDecoration : IDisposable
{
	public class DecorationUnlockData : IDecorationUnlockData
	{
		public readonly ReactiveProperty<bool> _isLocked;

		public ReactiveProperty<bool> IsLocked => _isLocked;

		public bool IsNotLockCondition { get; private set; }

		public DecorationUnlockData(bool isLocked, bool isNotLockCondition)
		{
			_isLocked = new ReactiveProperty<bool>(isLocked);
			IsNotLockCondition = isNotLockCondition;
		}
	}

	public interface IDecorationUnlockData
	{
		ReactiveProperty<bool> IsLocked { get; }

		bool IsNotLockCondition { get; }
	}

	public bool IsNeedGetNewAnnounce;

	private Dictionary<DecorationService.DecorationSkinType, DecorationUnlockData> _skinDic = new Dictionary<DecorationService.DecorationSkinType, DecorationUnlockData>();

	private MasterDataLoader _masterDataLoader;

	private UnlockConditionService _conditionService;

	private readonly Subject<(DecorationService.DecorationSkinType skinType, DecorationUnlockData unlockData)> _onLockStateChanged = new Subject<(DecorationService.DecorationSkinType, DecorationUnlockData)>();

	private DisposableBag _disposableBag;

	public Observable<(DecorationService.DecorationSkinType skinType, DecorationUnlockData unlockData)> OnLockStateChanged => _onLockStateChanged;

	public void Setup(MasterDataLoader masterDataLoader, UnlockConditionService conditionService)
	{
		_masterDataLoader = masterDataLoader;
		_conditionService = conditionService;
		DecorationSkinMasterData[] decorationSkins = _masterDataLoader.DecorationMaster.DecorationSkins;
		foreach (DecorationSkinMasterData decorationSkinMasterData in decorationSkins)
		{
			DecorationService.DecorationSkinType skinType = decorationSkinMasterData.SkinType;
			(bool unlocked, bool notLockCondition) tuple = _conditionService.IsUnlocked(skinType);
			bool item = tuple.unlocked;
			bool item2 = tuple.notLockCondition;
			DecorationUnlockData data = new DecorationUnlockData(!item, item2);
			_skinDic.Add(skinType, data);
			ObservableSubscribeExtensions.Subscribe(data._isLocked, delegate
			{
				_onLockStateChanged.OnNext((skinType, data));
			}).AddTo(ref _disposableBag);
		}
	}

	public IDecorationUnlockData GetLockState(DecorationService.DecorationSkinType skinType)
	{
		return _skinDic[skinType];
	}

	public void UnlockUpdate(UnlockItemService.ConditionsType conditionsType, string arg1, string arg2 = "0", string arg3 = "0")
	{
		bool flag = false;
		if (conditionsType == UnlockItemService.ConditionsType.Scenario)
		{
			IEnumerable<UnlockDecorationData> enumerable = _masterDataLoader.UnlockDecorationMasterList.Where((UnlockDecorationData x) => x.ConditionsType == UnlockItemService.ConditionsType.Scenario.ToName() && x.Arg1 == arg1);
			if (enumerable != null)
			{
				foreach (UnlockDecorationData item in enumerable)
				{
					DecorationService.DecorationSkinType key = FastEnum.Parse<DecorationService.DecorationSkinType>(item.ItemType);
					if (_skinDic[key]._isLocked.Value)
					{
						_skinDic[key]._isLocked.Value = false;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			IsNeedGetNewAnnounce = true;
			SaveDataManager.Instance.SaveDecorationProgressData();
		}
	}

	public void UnlockUpdateByData()
	{
		foreach (var (itemType, decorationUnlockData2) in _skinDic)
		{
			decorationUnlockData2._isLocked.Value = !_conditionService.IsUnlocked(itemType).unlocked;
		}
	}

	public bool IsPurchasableType(DecorationService.DecorationSkinType decorationType, out int price)
	{
		return _conditionService.IsPurchasableItem(decorationType, out price);
	}

	public bool CanPurchase(DecorationService.DecorationSkinType decorationType)
	{
		return _conditionService.CanPurchase(decorationType);
	}

	public bool Purchase(DecorationService.DecorationSkinType decorationType)
	{
		if (!_conditionService.Purchase(decorationType))
		{
			return false;
		}
		_skinDic[decorationType]._isLocked.Value = false;
		return true;
	}

	public void Dispose()
	{
		_disposableBag.Dispose();
	}
}
