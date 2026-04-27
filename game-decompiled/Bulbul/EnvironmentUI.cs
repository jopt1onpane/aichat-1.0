using System;
using System.Linq;
using DG.Tweening;
using MagicLightmapSwitcher;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class EnvironmentUI : MonoBehaviour
{
	[Inject]
	private ChangeOrderService _changeOrderService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private IOnClickButtonAllUIDeactivateProvider _onClickButtonAllUIDeactivateProvider;

	[Inject]
	private IAutoTimeWindowViewChanger _autoTimeWindowViewChanger;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("環境リストオブジェクト")]
	private GameObject _enviromentListParentObject;

	[SerializeField]
	private EnvironmentListService _environmentListService;

	[SerializeField]
	[Header("環境ScrollRect")]
	private ScrollRect _scrollRect;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("新規環境アイコンUI")]
	private NewEnvironmentMarkUI _newEnviromentMarkUI;

	[SerializeField]
	[Header("環境プリセット変更UI")]
	private LayoutPresetChangeUI _environmentPresetChangeUI;

	[SerializeField]
	[Header("日中ボタン")]
	private TimeEnvironmentController _timeDayController;

	[SerializeField]
	[Header("日没ボタン")]
	private TimeEnvironmentController _timeSunsetController;

	[SerializeField]
	[Header("夜ボタン")]
	private TimeEnvironmentController _timeNightController;

	[SerializeField]
	[Header("曇りボタン")]
	private TimeEnvironmentController _timeCloudyController;

	[SerializeField]
	private PurchasePopover _purchasePopover;

	[SerializeField]
	private PlayerPointView _ownPointView;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	[SerializeField]
	private Button _hideButton;

	[SerializeField]
	private StoredLightingScenario lightingScenario;

	private EnvironmentControllerBase[] _environmentControllers;

	private InteractableUI _currentMouseOverUI;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private readonly ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>();

	private RuntimeAPI lightMapSwitcher = new RuntimeAPI();

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public void Setup()
	{
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_newEnviromentMarkUI.Setup();
		_environmentPresetChangeUI.Setup(_environmentDataService);
		_environmentListService.Setup();
		_purchasePopover.Setup(_scrollRect);
		ObservableSubscribeExtensions.Subscribe(_hideButton.OnClickAsObservable(), delegate
		{
			_onClickButtonAllUIDeactivateProvider.OnClickButtonAllUIDeactivate();
		}).AddTo(this);
		_ownPointView.SetPoint(_playerPointService.Point, withAnimation: false);
		ObservableSubscribeExtensions.Subscribe(_playerPointService.OnPointChange, delegate
		{
			_ownPointView.SetPoint(_playerPointService.Point, withAnimation: true);
		}).AddTo(this);
		_environmentControllers = base.gameObject.GetComponentsInChildren<EnvironmentControllerBase>(includeInactive: true);
		EnvironmentControllerBase[] environmentControllers = _environmentControllers;
		for (int num = 0; num < environmentControllers.Length; num++)
		{
			environmentControllers[num].DeactivateNewIcon();
		}
		foreach (EnvironmentController item in _environmentControllers.OfType<EnvironmentController>())
		{
			item.Setup(_purchasePopover);
		}
		_timeDayController.Setup();
		_timeSunsetController.Setup();
		_timeNightController.Setup();
		_timeCloudyController.Setup();
		ObservableSubscribeExtensions.Subscribe(_timeDayController.OnClickMainIcon(), delegate
		{
			OnClickButtonChangeTime(EnvironmentType.Day);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_timeSunsetController.OnClickMainIcon(), delegate
		{
			OnClickButtonChangeTime(EnvironmentType.Sunset);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_timeNightController.OnClickMainIcon(), delegate
		{
			OnClickButtonChangeTime(EnvironmentType.Night);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_timeCloudyController.OnClickMainIcon(), delegate
		{
			OnClickButtonChangeTime(EnvironmentType.Cloudy);
		}).AddTo(this);
		AmbientSoundBehavior[] ambientSoundBehaviors = base.gameObject.GetComponentsInChildren<AmbientSoundBehavior>(includeInactive: true);
		ObservableSubscribeExtensions.Subscribe(_environmentPresetChangeUI.OnChangeCurrentData, delegate
		{
			ApplyWindowBySavedata();
			EnvironmentControllerBase[] environmentControllers2 = _environmentControllers;
			for (int i = 0; i < environmentControllers2.Length; i++)
			{
				environmentControllers2[i].ApplyWindowBySaveData();
			}
			AmbientSoundBehavior[] array = ambientSoundBehaviors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ApplySaveData();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
		}).AddTo(this);
		Deactivate();
		SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Subscribe(delegate(bool isActive)
		{
			if (isActive)
			{
				_timeDayController.OnDeactivateWindow();
				_timeSunsetController.OnDeactivateWindow();
				_timeNightController.OnDeactivateWindow();
				_timeCloudyController.OnDeactivateWindow();
			}
			else
			{
				WindowViewType[] array = new WindowViewType[4]
				{
					WindowViewType.Day,
					WindowViewType.Sunset,
					WindowViewType.Night,
					WindowViewType.Cloudy
				};
				foreach (WindowViewType windowViewType in array)
				{
					if (_environmentDataService.IsWindowActive(windowViewType))
					{
						if (windowViewType == WindowViewType.Day)
						{
							_timeDayController.ActivateWindow();
						}
						if (windowViewType == WindowViewType.Sunset)
						{
							_timeSunsetController.ActivateWindow();
						}
						if (windowViewType == WindowViewType.Night)
						{
							_timeNightController.ActivateWindow();
						}
						if (windowViewType == WindowViewType.Cloudy)
						{
							_timeCloudyController.ActivateWindow();
						}
						break;
					}
				}
			}
		}).AddTo(this);
	}

	public void ApplyWindowBySavedata()
	{
		EnvironmentControllerBase[] environmentControllers = _environmentControllers;
		for (int i = 0; i < environmentControllers.Length; i++)
		{
			environmentControllers[i].ApplyWindowBySaveData();
		}
		ApplyWindowTime();
	}

	private void ApplyWindowTime()
	{
		if (SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value)
		{
			_autoTimeWindowViewChanger.ApplyTimeOfDayFromCurrentTime();
		}
		else if (_environmentDataService.IsWindowActive(WindowViewType.Day))
		{
			ChangeTime(EnvironmentType.Day);
			_timeDayController.OnActivateWindow();
		}
		else if (_environmentDataService.IsWindowActive(WindowViewType.Sunset))
		{
			ChangeTime(EnvironmentType.Sunset);
			_timeSunsetController.OnActivateWindow();
		}
		else if (_environmentDataService.IsWindowActive(WindowViewType.Night))
		{
			ChangeTime(EnvironmentType.Night);
			_timeNightController.OnActivateWindow();
		}
		else if (_environmentDataService.IsWindowActive(WindowViewType.Cloudy))
		{
			ChangeTime(EnvironmentType.Cloudy);
			_timeCloudyController.OnActivateWindow();
		}
		else
		{
			ChangeTime(EnvironmentType.Sunset);
			_timeSunsetController.OnActivateWindow();
		}
	}

	public void Activate()
	{
		_isActive.Value = true;
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Environment);
		_isActive.Value = true;
		_facilityOpenButton.ActivateUseUI();
		_enviromentListParentObject.SetActive(value: true);
		_environmentListService.AdjustRectSize();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.verticalNormalizedPosition = 1f;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_isActive.Value = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_enviromentListParentObject.SetActive(value: false);
		});
	}

	public void OnClickButtonChangeTime(EnvironmentType environmentType)
	{
		if (!_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
		{
			WindowViewType viewType = environmentType switch
			{
				EnvironmentType.Day => WindowViewType.Day, 
				EnvironmentType.Sunset => WindowViewType.Sunset, 
				EnvironmentType.Night => WindowViewType.Night, 
				EnvironmentType.Cloudy => WindowViewType.Cloudy, 
				_ => throw new ArgumentOutOfRangeException("environmentType", environmentType, null), 
			};
			if (SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value)
			{
				SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value = false;
				SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
			}
			else if (_windowViewService.IsActiveWindow(viewType))
			{
				return;
			}
			ChangeTime(environmentType);
		}
	}

	private void ChangeTime(EnvironmentType environmentType)
	{
		if (lightingScenario != null)
		{
			lightMapSwitcher.SwitchLightmap((int)environmentType, lightingScenario);
		}
		bool flag = environmentType == EnvironmentType.Day;
		if (flag)
		{
			_timeDayController.ActivateWindow();
		}
		else
		{
			_timeDayController.OnDeactivateWindow();
		}
		_environmentDataService.SetViewActive(WindowViewType.Day, flag);
		bool flag2 = environmentType == EnvironmentType.Sunset;
		if (flag2)
		{
			_timeSunsetController.ActivateWindow();
		}
		else
		{
			_timeSunsetController.OnDeactivateWindow();
		}
		_environmentDataService.SetViewActive(WindowViewType.Sunset, flag2);
		bool flag3 = environmentType == EnvironmentType.Night;
		if (flag3)
		{
			_timeNightController.ActivateWindow();
		}
		else
		{
			_timeNightController.OnDeactivateWindow();
		}
		_environmentDataService.SetViewActive(WindowViewType.Night, flag3);
		bool flag4 = environmentType == EnvironmentType.Cloudy;
		if (flag4)
		{
			_timeCloudyController.ActivateWindow();
		}
		else
		{
			_timeCloudyController.OnDeactivateWindow();
		}
		_environmentDataService.SetViewActive(WindowViewType.Cloudy, flag4);
		WindowViewType weatherAndTimeType = environmentType switch
		{
			EnvironmentType.Day => WindowViewType.Day, 
			EnvironmentType.Sunset => WindowViewType.Sunset, 
			EnvironmentType.Night => WindowViewType.Night, 
			EnvironmentType.Cloudy => WindowViewType.Cloudy, 
			_ => throw new ArgumentOutOfRangeException("environmentType", environmentType, null), 
		};
		_windowViewService.ChangeWeatherAndTime(weatherAndTimeType);
		SaveDataManager.Instance.SaveEnviromentThrottled();
	}
}
