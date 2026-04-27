using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentTimeSelectorPresenter : MonoBehaviour
{
	[SerializeField]
	private EnvironmentTimeSelectorUI _view;

	[SerializeField]
	private AutoTimeWindowViewChangerForMobile _autoTimeWindowViewChanger;

	[Inject]
	private EnvironmentApplicationService _applicationService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ScenarioReader _scenarioReader;

	private Dictionary<EnvironmentType, EnvironmentTimeItemModel> _itemModels;

	public void Initialize()
	{
		_itemModels = new Dictionary<EnvironmentType, EnvironmentTimeItemModel>();
		foreach (EnvironmentMasterData item in from env in _masterDataLoader.EnvironmentMaster.Environments
			where env.EnvironmentType.IsTimeType()
			orderby env.SortOrder
			select env)
		{
			item.EnvironmentType.TryConvertToWindowViewType(out var windowViewType);
			_itemModels.Add(item.EnvironmentType, new EnvironmentTimeItemModel
			{
				EnvironmentType = item.EnvironmentType,
				IsUse = _environmentDataService.IsWindowActive(windowViewType),
				IsLocked = _unlockItemService.Environment.GetLockState(item.EnvironmentType).IsLocked.CurrentValue,
				IsNew = (!_environmentDataService.HavePlayed(item.EnvironmentType) && !_unlockItemService.Environment.GetLockState(item.EnvironmentType).IsNotLockCondition)
			});
		}
		_autoTimeWindowViewChanger.Setup();
		_autoTimeWindowViewChanger.IsActive.Subscribe(delegate(bool active)
		{
			_view.SetUseAutoSetting(active);
		}).AddTo(this);
		_view.Initialize(_itemModels.Values);
		_view.OnClickItem.Subscribe(delegate(EnvironmentType environmentType)
		{
			if (!_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
			{
				_systemSeService.PlaySelect();
				if (IsUnlockNight())
				{
					bool currentValue = SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue;
					SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value = false;
					_view.ActiveAuto = false;
					if (currentValue)
					{
						SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
						_view.PlayAutoObjExpandShrinkAnim(active: false);
					}
				}
				ChangeTimeActive(environmentType);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_view.OnClickAutoItem, delegate
		{
			if (IsUnlockNight())
			{
				bool flag2 = !SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue;
				if (flag2)
				{
					_systemSeService.PlaySelect();
				}
				else
				{
					_systemSeService.PlayCancelSelect();
				}
				SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value = flag2;
				SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
				_view.ActiveAuto = flag2;
				_view.SetUseAuto(flag2);
				foreach (var (environmentType2, _) in _itemModels)
				{
					_view.ReapplyModel(environmentType2);
				}
				_view.PlayAutoObjExpandShrinkAnim(flag2);
			}
		}).AddTo(this);
		_scenarioReader.OnEndStory.Subscribe(delegate(ScenarioType type)
		{
			if (type == ScenarioType.MainScenario)
			{
				bool flag2 = IsUnlockNight();
				_view.SetActiveAutoButton(IsUnlockNight());
				if (_view.EnabledAuto != flag2)
				{
					_view.EnabledAuto = flag2;
					_view.ActiveAuto = SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue;
					if (_view.IsExpanded)
					{
						_view.ExpandImmediate();
					}
					else
					{
						_view.ShrinkImmediate();
					}
				}
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_view.OnClickAutoSetting, delegate
		{
			if (IsUnlockNight() && !_autoTimeWindowViewChanger.IsActive.CurrentValue)
			{
				_systemSeService.PlaySelect();
				_autoTimeWindowViewChanger.ActivateSetting();
			}
		}).AddTo(this);
		foreach (KeyValuePair<EnvironmentType, EnvironmentTimeItemModel> itemModel in _itemModels)
		{
			itemModel.Deconstruct(out var key, out var value);
			EnvironmentType envType = key;
			EnvironmentTimeItemModel model = value;
			UnlockEnvironment.IEnvironmentUnlockData lockState = _unlockItemService.Environment.GetLockState(envType);
			if (!lockState.IsNotLockCondition)
			{
				lockState.IsLocked.Skip(1).Subscribe(delegate(bool isLock)
				{
					model.IsLocked = isLock;
					_view.ReapplyModel(envType);
				}).AddTo(this);
			}
		}
		bool flag = IsUnlockNight();
		_view.EnabledAuto = flag;
		_view.SetUseAuto(SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue);
		_view.ActiveAuto = SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue;
		_view.SetActiveAutoButton(flag);
		_view.ShrinkImmediate();
	}

	private bool IsUnlockNight()
	{
		if (!RoomLifetimeScope.Resolve<UnlockItemService>().Environment.GetLockState(EnvironmentType.Night).IsLocked.CurrentValue)
		{
			return true;
		}
		return false;
	}

	public void ApplySavedata()
	{
		foreach (EnvironmentTimeItemModel value in _itemModels.Values)
		{
			value.EnvironmentType.TryConvertToWindowViewType(out var windowViewType);
			value.IsUse = _environmentDataService.IsWindowActive(windowViewType);
			value.IsLocked = _unlockItemService.Environment.GetLockState(value.EnvironmentType).IsLocked.CurrentValue;
			value.IsNew = !_environmentDataService.HavePlayed(value.EnvironmentType) && !_unlockItemService.Environment.GetLockState(value.EnvironmentType).IsNotLockCondition;
			_view.ReapplyModel(value.EnvironmentType);
		}
	}

	private void ChangeTimeActive(EnvironmentType activateEnvType)
	{
		foreach (EnvironmentTimeItemModel value in _itemModels.Values)
		{
			value.EnvironmentType.TryConvertToWindowViewType(out var windowViewType);
			value.IsUse = value.EnvironmentType == activateEnvType;
			_environmentDataService.SetViewActive(windowViewType, value.IsUse);
			if (value.IsUse && value.IsNew)
			{
				_environmentDataService.SetPlayed(value.EnvironmentType);
				value.IsNew = false;
			}
			_view.ReapplyModel(value.EnvironmentType);
			_applicationService.ApplyWindow(windowViewType, value.IsUse);
		}
		_view.SetUseAuto(SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue);
	}

	public void ExpandShrinkImmediate()
	{
		if (_view.IsExpanded)
		{
			_view.ExpandImmediate();
		}
		else
		{
			_view.ShrinkImmediate();
		}
	}
}
