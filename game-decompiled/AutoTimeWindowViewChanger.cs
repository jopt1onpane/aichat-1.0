using System;
using Bulbul;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class AutoTimeWindowViewChanger : MonoBehaviour, IAutoTimeWindowViewChanger
{
	[Inject]
	private DateService _dateService;

	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[SerializeField]
	[Header("オート切り替えボタン")]
	private Button _autoSwitchButton;

	[SerializeField]
	[Header("オート切り替えボタン")]
	private InteractableUI _autoSwitchButtonInteractableUI;

	[SerializeField]
	[Header("設定ボタンのUICanvasGroup")]
	private CanvasGroup _settingOpenButtonCanvasGroup;

	[SerializeField]
	[Header("設定画面を開くボタン")]
	private Button _settingOpenButton;

	[SerializeField]
	[Header("設定画面を開くボタンのInteractableUI")]
	private InteractableUI _settingOpenButtonInteractableUI;

	[SerializeField]
	[Header("1話読了前非表示用：時間帯UIの親")]
	private RectTransform _timButtonUIParent;

	[SerializeField]
	[Header("設定ウィンドウの操作用")]
	private AutoTimeWindowView _autoTimeWindowView;

	private Tween _fadeTweenSettingButton;

	private RectTransform _rectTransformSettingButton;

	private float _fromPosXForSettingButton;

	private float _toPosXForSettingButton;

	private Tween _moveTweenSettingButton;

	private bool _isPossibleUseAutoChanger = true;

	private bool _isActive;

	public void Setup()
	{
		_settingOpenButtonInteractableUI.Setup();
		_autoSwitchButtonInteractableUI.Setup();
		_autoTimeWindowView.Setup();
		AutoTimeWindowChangeData saveData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		ObservableSubscribeExtensions.Subscribe(_autoTimeWindowView.OnOutsideClick, delegate
		{
			DeactivateUI();
		}).AddTo(this);
		_dateService.OnChangeTime.Subscribe(delegate(DateTime dateTime)
		{
			if (saveData.IsActiveAuto.Value && _isPossibleUseAutoChanger)
			{
				float timeHours = (float)dateTime.Hour + (float)dateTime.Minute / 60f + (float)dateTime.Second / 3600f;
				WindowViewType windowViewTypeFromTime = _autoTimeWindowView.Settings.GetWindowViewTypeFromTime(timeHours);
				if (!TryGetCurrentTimeOfDayType(out var viewType) || viewType != windowViewTypeFromTime)
				{
					_windowViewService.ChangeWeatherAndTime(windowViewTypeFromTime);
				}
			}
		}).AddTo(this);
		_autoTimeWindowView.OnSave.Subscribe(delegate(AutoTimeWindowSettings data)
		{
			SaveAutoTimeWindowChangeData(data);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_settingOpenButton.OnClickAsObservable(), delegate
		{
			if (_isActive)
			{
				_systemSeService.PlayCancel();
				DeactivateUI();
			}
			else
			{
				_systemSeService.PlayClick();
				ActivateUI();
			}
		}).AddTo(this);
		_rectTransformSettingButton = _settingOpenButton.gameObject.transform as RectTransform;
		RectTransform rectTransform = _autoSwitchButton.transform as RectTransform;
		Canvas.ForceUpdateCanvases();
		_fromPosXForSettingButton = rectTransform.anchoredPosition.x;
		_toPosXForSettingButton = _rectTransformSettingButton.anchoredPosition.x;
		_rectTransformSettingButton.anchoredPosition = new Vector2(_fromPosXForSettingButton, _rectTransformSettingButton.anchoredPosition.y);
		_settingOpenButtonCanvasGroup.alpha = 0f;
		_settingOpenButton.gameObject.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(_autoSwitchButton.OnClickAsObservable(), delegate
		{
			saveData.IsActiveAuto.Value = !saveData.IsActiveAuto.Value;
			SaveAutoTimeWindowChangeData(_autoTimeWindowView.Settings);
			if (saveData.IsActiveAuto.Value)
			{
				_systemSeService.PlayClick();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Subscribe(delegate(bool isActive)
		{
			if (isActive)
			{
				ActivateSettingButton();
			}
			else
			{
				RestoreWindowViewFromSaveData();
				DeactivateSettingButton();
			}
		}).AddTo(this);
		if (SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value)
		{
			ActivateSettingButton();
			_autoTimeWindowView.RefreshFromSaveData();
			ApplyTimeOfDayFromCurrentTime();
		}
		else
		{
			DeactivateSettingButton();
		}
		if (IsUnlockNight())
		{
			_autoSwitchButtonInteractableUI.gameObject.SetActive(value: true);
			return;
		}
		float x = 367.8f;
		_timButtonUIParent.sizeDelta = new Vector2(x, _timButtonUIParent.sizeDelta.y);
		_autoSwitchButtonInteractableUI.gameObject.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(_scenarioReader.OnEndStory, delegate
		{
			if (IsUnlockNight())
			{
				float x2 = 417.9f;
				_timButtonUIParent.sizeDelta = new Vector2(x2, _timButtonUIParent.sizeDelta.y);
				_autoSwitchButtonInteractableUI.gameObject.SetActive(value: true);
			}
		}).AddTo(this);
		static bool IsUnlockNight()
		{
			if (!RoomLifetimeScope.Resolve<UnlockItemService>().Environment.GetLockState(EnvironmentType.Night).IsLocked.CurrentValue)
			{
				return true;
			}
			return false;
		}
		bool TryGetCurrentTimeOfDayType(out WindowViewType viewType)
		{
			if (_windowViewService.IsActiveWindow(WindowViewType.Day))
			{
				viewType = WindowViewType.Day;
				return true;
			}
			if (_windowViewService.IsActiveWindow(WindowViewType.Sunset))
			{
				viewType = WindowViewType.Sunset;
				return true;
			}
			if (_windowViewService.IsActiveWindow(WindowViewType.Night))
			{
				viewType = WindowViewType.Night;
				return true;
			}
			viewType = WindowViewType.Fireworks;
			return false;
		}
	}

	private void ActivateSettingButton()
	{
		_settingOpenButton.gameObject.SetActive(value: true);
		_moveTweenSettingButton?.Kill();
		_fadeTweenSettingButton?.Kill();
		_moveTweenSettingButton = _rectTransformSettingButton.DOAnchorPosX(_toPosXForSettingButton, 0.2f);
		_fadeTweenSettingButton = _settingOpenButtonCanvasGroup.DOFade(1f, 0.2f);
		_autoSwitchButtonInteractableUI.ActivateUseUI();
	}

	private void DeactivateSettingButton()
	{
		_moveTweenSettingButton?.Kill();
		_fadeTweenSettingButton?.Kill();
		_moveTweenSettingButton = _rectTransformSettingButton.DOAnchorPosX(_fromPosXForSettingButton, 0.2f);
		_fadeTweenSettingButton = _settingOpenButtonCanvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_settingOpenButton.gameObject.SetActive(value: false);
		});
		_autoSwitchButtonInteractableUI.DeactivateUseUI();
		DeactivateUI();
	}

	private void ActivateUI()
	{
		_isActive = true;
		_autoTimeWindowView.ActivateUI();
		_settingOpenButtonInteractableUI?.ActivateUseUI();
	}

	private void DeactivateUI()
	{
		_isActive = false;
		_autoTimeWindowView.DeactivateUI();
		_settingOpenButtonInteractableUI?.DeactivateUseUI();
	}

	private void SaveAutoTimeWindowChangeData(AutoTimeWindowSettings settings)
	{
		AutoTimeWindowChangeData autoTimeWindowChangeData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		autoTimeWindowChangeData.TimeDayStart = settings.TimeDayStart;
		autoTimeWindowChangeData.TimeSunsetStart = settings.TimeSunsetStart;
		autoTimeWindowChangeData.TimeNightStart = settings.TimeNightStart;
		SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
	}

	private void RestoreWindowViewFromSaveData()
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
				_windowViewService.ChangeWeatherAndTime(windowViewType);
				return;
			}
		}
		_windowViewService.ChangeWeatherAndTime(WindowViewType.Day);
	}

	public void ApplyTimeOfDayFromCurrentTime()
	{
		if (SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value)
		{
			_autoTimeWindowView.RefreshFromSaveData();
			DateTime now = DateTime.Now;
			float timeHours = (float)now.Hour + (float)now.Minute / 60f + (float)now.Second / 3600f;
			WindowViewType windowViewTypeFromTime = _autoTimeWindowView.Settings.GetWindowViewTypeFromTime(timeHours);
			_windowViewService.ChangeWeatherAndTime(windowViewTypeFromTime);
		}
	}

	public void SetPossibleUseAutoChanger(bool isPossible)
	{
		_isPossibleUseAutoChanger = isPossible;
	}
}
