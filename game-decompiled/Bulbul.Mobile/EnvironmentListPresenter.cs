using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using FastEnumUtility;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentListPresenter : MonoBehaviour
{
	[SerializeField]
	private EnvironmentListView _list;

	[SerializeField]
	private EnvironmentPurchaseDialog _purchaseDialog;

	[SerializeField]
	private SimpleNoticeDialog _mobileDemoEditionLockedNoticeDialog;

	[Inject]
	private EnvironmentApplicationService _applicationService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private MobileDemoEditionLockedTargetCheckService _mobileDemoEditionLockedTargetCheckService;

	[Inject]
	private SystemSeService _systemSeService;

	private readonly Subject<EnvironmentType> _onChangeActiveEnvironment = new Subject<EnvironmentType>();

	private bool _lastCheckedUpgradePassPurchasedState;

	public Observable<EnvironmentType> OnChangeActiveEnvironment => _onChangeActiveEnvironment;

	public bool LastCheckedUpgradePassPurchasedState => _lastCheckedUpgradePassPurchasedState;

	public void SetLastCheckedUpgradePassPurchasedState(bool purchased)
	{
		_lastCheckedUpgradePassPurchasedState = purchased;
	}

	public void Initialize()
	{
		_purchaseDialog.Initialize();
		_list.Initialize();
		_list.OnClickItemMainButton.Subscribe(delegate(EnvironmentType envType)
		{
			ToggleWindowAndSoundActive(envType);
		}).AddTo(this);
		_list.OnClickItemWindowButton.Subscribe(delegate(EnvironmentType envType)
		{
			ToggleWindowActive(envType);
		}).AddTo(this);
		_list.OnClickPurchaseButton.Subscribe(delegate(EnvironmentType envType)
		{
			_systemSeService.PlaySelect();
			Purchase(envType);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_list.OnClickMobileDemoEditionLocked, delegate
		{
			_systemSeService.PlaySelect();
			_mobileDemoEditionLockedNoticeDialog.Activate();
		}).AddTo(this);
		_unlockItemService.Environment.OnLockStateChanged.Subscribe(delegate((EnvironmentType environmentType, UnlockEnvironment.EnvironmentUnlockData unlockData) x)
		{
			var (environmentItemModel, modelIndex) = GetItemModelFromList(x.environmentType);
			if (environmentItemModel != null)
			{
				environmentItemModel.LockState = GetLockState(x.environmentType, out var _);
				_list.ReapplyModel(x.environmentType.GetEnvironmentControllerType(), modelIndex, lockStateChanged: true);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_playerPointService.OnPointChange, delegate
		{
			foreach (var allModel in _list.GetAllModels())
			{
				EnvironmentControllerType item = allModel.controllerType;
				IReadOnlyList<EnvironmentItemModel> item2 = allModel.models;
				for (int i = 0; i < item2.Count; i++)
				{
					EnvironmentItemModel environmentItemModel = item2[i];
					if (environmentItemModel.LockState == EnvironmentItemModel.ItemLockState.LockedByPurchase)
					{
						bool hasEnoughPoints = environmentItemModel.HasEnoughPoints;
						environmentItemModel.HasEnoughPoints = _unlockItemService.Environment.CanPurchase(environmentItemModel.EnvironmentType);
						if (environmentItemModel.HasEnoughPoints != hasEnoughPoints)
						{
							_list.ReapplyModel(item, i, lockStateChanged: false);
						}
					}
				}
			}
		}).AddTo(this);
		_purchaseDialog.OnPurchased.Subscribe(delegate(EnvironmentType environmentType)
		{
			SetWindowAndSoundActive(environmentType, active: true);
		}).AddTo(this);
	}

	private IEnumerable<EnvironmentMasterData> GetTargetEnvironmentMasters()
	{
		return _masterDataLoader.EnvironmentMaster.Environments.Where((EnvironmentMasterData env) => !env.EnvironmentType.IsTimeType());
	}

	private EnvironmentItemModel CreateItemModel(EnvironmentMasterData master)
	{
		EnvironmentType environmentType = master.EnvironmentType;
		bool isWindowActive = false;
		if (environmentType.TryConvertToWindowViewType(out var windowViewType))
		{
			isWindowActive = _environmentDataService.IsWindowActive(windowViewType);
		}
		float volume = 0f;
		if (environmentType.TryConvertToAmbientSoundType(out var ambientSoundType))
		{
			(float volume, bool isMute) volume2 = _environmentDataService.GetVolume(ambientSoundType);
			(volume, _) = volume2;
			if (volume2.isMute)
			{
				volume = 0f;
			}
		}
		int price;
		EnvironmentItemModel.ItemLockState lockState = GetLockState(environmentType, out price);
		bool isMobileDemoEditionLocked = false;
		return new EnvironmentItemModel
		{
			IsMobileDemoEditionLocked = isMobileDemoEditionLocked,
			EnvironmentType = environmentType,
			SortOrder = master.SortOrder,
			NameLocalizeID = master.NameLocalizeID,
			IsWindowActive = isWindowActive,
			Volume = volume,
			LockState = lockState,
			Price = price,
			HasEnoughPoints = _unlockItemService.Environment.CanPurchase(environmentType),
			IsNew = (!_environmentDataService.HavePlayed(environmentType) && !_unlockItemService.Environment.GetLockState(environmentType).IsNotLockCondition)
		};
	}

	private bool IsExpired(EnvironmentType environmentType)
	{
		if (GetLockState(environmentType, out var _) == EnvironmentItemModel.ItemLockState.Unlocked)
		{
			return false;
		}
		UnlockEnvironmentData unlockEnvironmentData = _masterDataLoader.UnlockEnvironmentMasterList.FirstOrDefault((UnlockEnvironmentData x) => x.ItemType == environmentType.ToName());
		if (unlockEnvironmentData == null)
		{
			return false;
		}
		if (!unlockEnvironmentData.TryGetUnlockExpire(out var dateTime))
		{
			return false;
		}
		return DateTime.Now > dateTime;
	}

	private (EnvironmentItemModel model, int index) GetItemModelFromList(EnvironmentType environmentType)
	{
		return _list.GetModel(environmentType.GetEnvironmentControllerType(), environmentType);
	}

	private EnvironmentItemModel.ItemLockState GetLockState(EnvironmentType environmentType, out int price)
	{
		price = 0;
		if (!_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
		{
			return EnvironmentItemModel.ItemLockState.Unlocked;
		}
		if (_unlockItemService.Environment.IsPurchasableType(environmentType, out price))
		{
			return EnvironmentItemModel.ItemLockState.LockedByPurchase;
		}
		return EnvironmentItemModel.ItemLockState.Locked;
	}

	private void ToggleWindowAndSoundActive(EnvironmentType environmentType)
	{
		EnvironmentItemModel item = GetItemModelFromList(environmentType).model;
		if (item != null)
		{
			bool active = !item.IsWindowActive && !item.IsSoundActive;
			SetWindowAndSoundActive(environmentType, active);
		}
	}

	private void SetWindowAndSoundActive(EnvironmentType environmentType, bool active)
	{
		var (environmentItemModel, modelIndex) = GetItemModelFromList(environmentType);
		if (environmentItemModel == null)
		{
			return;
		}
		WindowViewType windowViewType;
		bool flag = environmentType.TryConvertToWindowViewType(out windowViewType);
		if (flag)
		{
			environmentItemModel.IsWindowActive = active;
			_environmentDataService.SetViewActive(windowViewType, environmentItemModel.IsWindowActive);
		}
		AmbientSoundType ambientSoundType;
		bool num = environmentType.TryConvertToAmbientSoundType(out ambientSoundType);
		if (num)
		{
			if (active)
			{
				float item = _environmentDataService.GetVolume(ambientSoundType).volume;
				environmentItemModel.Volume = ((item > 0f) ? item : 0.5f);
				_environmentDataService.SetVolume(ambientSoundType, environmentItemModel.Volume);
				_environmentDataService.SetMute(ambientSoundType, isMute: false);
			}
			else
			{
				environmentItemModel.Volume = 0f;
				_environmentDataService.SetMute(ambientSoundType, isMute: true);
			}
		}
		if (environmentItemModel.IsNew)
		{
			environmentItemModel.IsNew = false;
			_environmentDataService.SetPlayed(environmentType);
		}
		_list.ReapplyModel(environmentType.GetEnvironmentControllerType(), modelIndex, lockStateChanged: false);
		if (flag)
		{
			_applicationService.ApplyWindow(windowViewType, environmentItemModel.IsWindowActive);
		}
		if (num)
		{
			_applicationService.ApplySound(ambientSoundType, environmentItemModel.IsSoundActive, environmentItemModel.Volume);
		}
		_onChangeActiveEnvironment.OnNext(environmentType);
	}

	private void ToggleWindowActive(EnvironmentType environmentType)
	{
		if (!environmentType.TryConvertToWindowViewType(out var windowViewType))
		{
			return;
		}
		var (environmentItemModel, modelIndex) = GetItemModelFromList(environmentType);
		if (environmentItemModel != null)
		{
			environmentItemModel.IsWindowActive = !environmentItemModel.IsWindowActive;
			_environmentDataService.SetViewActive(windowViewType, environmentItemModel.IsWindowActive);
			if (environmentItemModel.IsNew)
			{
				environmentItemModel.IsNew = false;
				_environmentDataService.SetPlayed(environmentType);
			}
			_list.ReapplyModel(environmentType.GetEnvironmentControllerType(), modelIndex, lockStateChanged: false);
			_applicationService.ApplyWindow(windowViewType, environmentItemModel.IsWindowActive);
			_onChangeActiveEnvironment.OnNext(environmentType);
		}
	}

	private void ToggleSoundActive(EnvironmentType environmentType)
	{
		if (!environmentType.TryConvertToAmbientSoundType(out var ambientSoundType))
		{
			return;
		}
		var (environmentItemModel, modelIndex) = GetItemModelFromList(environmentType);
		if (environmentItemModel != null)
		{
			bool flag = !environmentItemModel.IsSoundActive;
			if (flag)
			{
				float item = _environmentDataService.GetVolume(ambientSoundType).volume;
				environmentItemModel.Volume = ((item > 0f) ? item : 0.5f);
				_environmentDataService.SetVolume(ambientSoundType, environmentItemModel.Volume);
				_environmentDataService.SetMute(ambientSoundType, !environmentItemModel.IsSoundActive);
			}
			else
			{
				environmentItemModel.Volume = 0f;
				_environmentDataService.SetMute(ambientSoundType, !environmentItemModel.IsSoundActive);
			}
			if (environmentItemModel.IsNew)
			{
				environmentItemModel.IsNew = false;
				_environmentDataService.SetPlayed(environmentType);
			}
			_list.ReapplyModel(environmentType.GetEnvironmentControllerType(), modelIndex, lockStateChanged: false);
			_applicationService.ApplySound(ambientSoundType, flag, environmentItemModel.Volume);
			_onChangeActiveEnvironment.OnNext(environmentType);
		}
	}

	private void Purchase(EnvironmentType environmentType)
	{
		_purchaseDialog.Open(environmentType);
	}

	private void ChangeSoundVolume(EnvironmentType environmentType, float volume, bool save)
	{
		if (!environmentType.TryConvertToAmbientSoundType(out var ambientSoundType))
		{
			return;
		}
		var (environmentItemModel, modelIndex) = GetItemModelFromList(environmentType);
		if (environmentItemModel != null)
		{
			environmentItemModel.Volume = volume;
			if (save)
			{
				_environmentDataService.SetVolume(ambientSoundType, volume);
			}
			_list.ReapplyModel(environmentType.GetEnvironmentControllerType(), modelIndex, lockStateChanged: false);
			_applicationService.ApplySound(ambientSoundType, environmentItemModel.IsSoundActive, volume);
		}
	}
}
