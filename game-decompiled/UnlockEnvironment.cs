using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using FastEnumUtility;
using R3;

public class UnlockEnvironment : IDisposable
{
	public class EnvironmentUnlockData : IEnvironmentUnlockData
	{
		public readonly ReactiveProperty<bool> _isLocked;

		public ReadOnlyReactiveProperty<bool> IsLocked => _isLocked;

		public bool IsNotLockCondition { get; private set; }

		public EnvironmentUnlockData(bool isLocked, bool isNotLockCondition)
		{
			_isLocked = new ReactiveProperty<bool>(isLocked);
			IsNotLockCondition = isNotLockCondition;
		}
	}

	public interface IEnvironmentUnlockData
	{
		ReadOnlyReactiveProperty<bool> IsLocked { get; }

		bool IsNotLockCondition { get; }
	}

	public bool IsNeedGetNewAnnounce;

	private readonly Dictionary<EnvironmentType, EnvironmentUnlockData> _environmentDic = new Dictionary<EnvironmentType, EnvironmentUnlockData>();

	private MasterDataLoader _masterDataLoader;

	private UnlockConditionService _conditionService;

	private readonly Subject<(EnvironmentType environmentType, EnvironmentUnlockData unlockData)> _onLockStateChanged = new Subject<(EnvironmentType, EnvironmentUnlockData)>();

	private DisposableBag _disposableBag;

	public Observable<(EnvironmentType environmentType, EnvironmentUnlockData unlockData)> OnLockStateChanged => _onLockStateChanged;

	public IEnumerable<(EnvironmentType environmentType, IEnvironmentUnlockData unlockData)> LockStates => _environmentDic.Select((KeyValuePair<EnvironmentType, EnvironmentUnlockData> x) => ((EnvironmentType Key, IEnvironmentUnlockData))(Key: x.Key, x.Value));

	public void Setup(MasterDataLoader masterDataLoader, UnlockConditionService conditionService)
	{
		_masterDataLoader = masterDataLoader;
		_conditionService = conditionService;
		EnvironmentMasterData[] environments = _masterDataLoader.EnvironmentMaster.Environments;
		foreach (EnvironmentMasterData environmentMasterData in environments)
		{
			EnvironmentType environmentType = environmentMasterData.EnvironmentType;
			(bool unlocked, bool notLockCondition) tuple = _conditionService.IsUnlocked(environmentType);
			bool item = tuple.unlocked;
			bool item2 = tuple.notLockCondition;
			EnvironmentUnlockData data = new EnvironmentUnlockData(!item, item2);
			_environmentDic.Add(environmentType, data);
			ObservableSubscribeExtensions.Subscribe(data._isLocked, delegate
			{
				_onLockStateChanged.OnNext((environmentType, data));
			}).AddTo(ref _disposableBag);
		}
	}

	public IEnvironmentUnlockData GetLockState(EnvironmentType environmentType)
	{
		return _environmentDic[environmentType];
	}

	public void UnlockUpdate(UnlockItemService.ConditionsType conditionsType, string arg1, string arg2 = "0", string arg3 = "0")
	{
		bool flag = false;
		if (conditionsType == UnlockItemService.ConditionsType.Scenario)
		{
			IEnumerable<UnlockEnvironmentData> enumerable = _masterDataLoader.UnlockEnvironmentMasterList.Where((UnlockEnvironmentData x) => x.ConditionsType == UnlockItemService.ConditionsType.Scenario.ToName() && x.Arg1 == arg1);
			if (enumerable != null)
			{
				foreach (UnlockEnvironmentData item in enumerable)
				{
					EnvironmentType key = FastEnum.Parse<EnvironmentType>(item.ItemType);
					if (_environmentDic[key]._isLocked.Value)
					{
						_environmentDic[key]._isLocked.Value = false;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			IsNeedGetNewAnnounce = true;
			SaveDataManager.Instance.SaveEnvironmentProgressData();
		}
	}

	public void UnlockUpdateByData()
	{
		foreach (var (itemType, environmentUnlockData2) in _environmentDic)
		{
			environmentUnlockData2._isLocked.Value = !_conditionService.IsUnlocked(itemType).unlocked;
		}
	}

	public bool IsPurchasableType(EnvironmentType environmentType, out int price)
	{
		return _conditionService.IsPurchasableItem(environmentType, out price);
	}

	public bool CanPurchase(EnvironmentType environmentType)
	{
		return _conditionService.CanPurchase(environmentType);
	}

	public bool Purchase(EnvironmentType environmentType)
	{
		if (!_conditionService.Purchase(environmentType))
		{
			return false;
		}
		_environmentDic[environmentType]._isLocked.Value = false;
		return true;
	}

	public void Dispose()
	{
		_disposableBag.Dispose();
	}
}
