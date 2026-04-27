using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityPomodoroTimerDisplayAndSetting : MonoBehaviour
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[SerializeField]
	private PomodoroTimerSettingView _pomodoroTimerSettingView;

	[SerializeField]
	[Header("ボタンも兼ねる")]
	private PomodoroTimerDisplayViewForMobile _pomodoroTimerDisplayView;

	[SerializeField]
	[Header("表示更新のみ 現状壁紙用のタイマー")]
	private PomodoroTimerDisplayViewForMobile[] _displayOnlyViews;

	private Subject<(bool isRequestedPortraitWallpaper, bool isNeedCloseSE)> _onClose = new Subject<(bool, bool)>();

	public Observable<Unit> OnClickOpenPomodoroTimerSettingButton => _pomodoroTimerDisplayView.OnClickOpenSettingButton;

	public Observable<(bool isRequestedPortraitWallpaper, bool isNeedCloseSE)> OnClose => _onClose;

	public bool IsActive { get; private set; }

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnStartPomodoro, delegate
		{
			StartDisplayTimers();
		}).AddTo(this);
		_pomodoroService.CurrentLoopCount.Subscribe(delegate(int currentLoopCount)
		{
			UpdateLoopCountDisplayTimers(currentLoopCount);
		}).AddTo(this);
		_pomodoroService.OnPlayPomodoro.Subscribe(delegate(PomodoroService.PomodoroType type)
		{
			PlayDisplayTimers(type);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
		{
			CompleteDisplayTimers();
		}).AddTo(this);
		_pomodoroService.OnUpdatePomodoro.Subscribe(delegate((PomodoroService.PomodoroType, TimeSpan, TimeSpan) info)
		{
			UpdateDisplayTimers(info);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnPause, delegate
		{
			PauseDisplayTimers();
		}).AddTo(this);
		_pomodoroService.OnUnpause.Subscribe(delegate(PomodoroService.PomodoroType pomodoroType)
		{
			UnPauseDisplayTimers(pomodoroType);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			LockDisplayTimers();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			UnLockDisplayTimers();
		}).AddTo(this);
		SaveDataManager.Instance.PomodoroData.WorkMinutes.Subscribe(delegate(int workMinutes)
		{
			UpdateWorkMinutes(workMinutes);
		}).AddTo(this);
		SaveDataManager.Instance.PomodoroData.LoopCount.Subscribe(delegate(int count)
		{
			UpdateLoopCount(count);
		}).AddTo(this);
		SetupSetting();
	}

	private void UpdateWorkMinutes(int workMinutes)
	{
		string text = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value).ToString("hh\\:mm\\:ss");
		_pomodoroTimerDisplayView.SetTimeText(text);
		_pomodoroTimerDisplayView.SetSettingTimeText(text);
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		foreach (PomodoroTimerDisplayViewForMobile obj in displayOnlyViews)
		{
			obj.SetTimeText(text);
			obj.SetSettingTimeText(text);
		}
	}

	private void UpdateLoopCount(int count)
	{
		_pomodoroTimerDisplayView.SetSettingLoopCountText(count);
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].SetSettingLoopCountText(count);
		}
	}

	private void StartDisplayTimers()
	{
		string timeText = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value).ToString("hh\\:mm\\:ss");
		int value = SaveDataManager.Instance.PomodoroData.LoopCount.Value;
		_pomodoroTimerDisplayView.SetTimeText(timeText);
		_pomodoroTimerDisplayView.SetRemainLoopCountText(1, value);
		_pomodoroTimerDisplayView.StateView.SwitchStartTimer();
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		foreach (PomodoroTimerDisplayViewForMobile obj in displayOnlyViews)
		{
			obj.SetTimeText(timeText);
			obj.SetRemainLoopCountText(1, value);
			obj.StateView.SwitchStartTimer();
		}
	}

	private void UpdateLoopCountDisplayTimers(int currentLoopCount)
	{
		_pomodoroTimerDisplayView.SetRemainLoopCountText(currentLoopCount, SaveDataManager.Instance.PomodoroData.LoopCount.Value);
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			_ = displayOnlyViews[i];
			_pomodoroTimerDisplayView.SetRemainLoopCountText(currentLoopCount, SaveDataManager.Instance.PomodoroData.LoopCount.Value);
		}
	}

	private void PlayDisplayTimers(PomodoroService.PomodoroType type)
	{
		_pomodoroTimerDisplayView.StateView.SwitchPlayTimer(type, delegate(PomodoroService.PomodoroType pomodoroType)
		{
			switch (pomodoroType)
			{
			case PomodoroService.PomodoroType.Work:
			{
				string text = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value).ToString("hh\\:mm\\:ss");
				_pomodoroTimerDisplayView.SetTimeText(text);
				_pomodoroTimerDisplayView.SetSettingTimeText(text);
				break;
			}
			case PomodoroService.PomodoroType.Break:
			{
				TimeSpan timeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value);
				_pomodoroTimerDisplayView.SetTimeText(timeSpan.ToString("hh\\:mm\\:ss"));
				break;
			}
			}
			int currentValue = _pomodoroService.CurrentLoopCount.CurrentValue;
			int value = SaveDataManager.Instance.PomodoroData.LoopCount.Value;
			_pomodoroTimerDisplayView.SetRemainLoopCountText(currentValue, value);
		});
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		foreach (PomodoroTimerDisplayViewForMobile display in displayOnlyViews)
		{
			display.StateView.SwitchPlayTimer(type, delegate(PomodoroService.PomodoroType pomodoroType)
			{
				switch (pomodoroType)
				{
				case PomodoroService.PomodoroType.Work:
				{
					string text = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value).ToString("hh\\:mm\\:ss");
					display.SetTimeText(text);
					display.SetSettingTimeText(text);
					break;
				}
				case PomodoroService.PomodoroType.Break:
				{
					TimeSpan timeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value);
					display.SetTimeText(timeSpan.ToString("hh\\:mm\\:ss"));
					break;
				}
				}
				int currentValue = _pomodoroService.CurrentLoopCount.CurrentValue;
				int value = SaveDataManager.Instance.PomodoroData.LoopCount.Value;
				display.SetRemainLoopCountText(currentValue, value);
			});
		}
	}

	private void CompleteDisplayTimers()
	{
		_pomodoroTimerDisplayView.StateView.SwitchCompleteTimer(null);
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].StateView.SwitchCompleteTimer(null);
		}
	}

	private void UpdateDisplayTimers((PomodoroService.PomodoroType, TimeSpan, TimeSpan) info)
	{
		if (_pomodoroTimerDisplayView.StateView.IsTransitioning)
		{
			return;
		}
		var (pomodoroType, timeSpan, timeSpan2) = info;
		switch (pomodoroType)
		{
		case PomodoroService.PomodoroType.Work:
			_pomodoroTimerDisplayView.SetMeterOverWriteImageFillAmount(1f - (float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds);
			break;
		case PomodoroService.PomodoroType.Break:
			_pomodoroTimerDisplayView.SetMeterOverWriteImageFillAmount((float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds);
			break;
		}
		_pomodoroTimerDisplayView.SetTimeText(timeSpan.ToString("hh\\:mm\\:ss"));
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		foreach (PomodoroTimerDisplayViewForMobile pomodoroTimerDisplayViewForMobile in displayOnlyViews)
		{
			if (pomodoroTimerDisplayViewForMobile.StateView.IsTransitioning)
			{
				break;
			}
			switch (pomodoroType)
			{
			case PomodoroService.PomodoroType.Work:
				pomodoroTimerDisplayViewForMobile.SetMeterOverWriteImageFillAmount(1f - (float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds);
				break;
			case PomodoroService.PomodoroType.Break:
				pomodoroTimerDisplayViewForMobile.SetMeterOverWriteImageFillAmount((float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds);
				break;
			}
			pomodoroTimerDisplayViewForMobile.SetTimeText(timeSpan.ToString("hh\\:mm\\:ss"));
		}
	}

	private void PauseDisplayTimers()
	{
		_pomodoroTimerDisplayView.StateView.SwitchPauseTimer();
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].StateView.SwitchPauseTimer();
		}
	}

	private void UnPauseDisplayTimers(PomodoroService.PomodoroType pomodoroType)
	{
		_pomodoroTimerDisplayView.StateView.SwitchUnPauseTimer(pomodoroType);
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].StateView.SwitchUnPauseTimer(pomodoroType);
		}
	}

	private void LockDisplayTimers()
	{
		_pomodoroTimerDisplayView.StateView.ActivateLockTimer();
		_pomodoroTimerDisplayView.DeactivateOpenButton();
		if (_pomodoroTimerSettingView.IsActive)
		{
			_onClose.OnNext((false, false));
		}
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].StateView.ActivateLockTimer();
		}
	}

	private void UnLockDisplayTimers()
	{
		_pomodoroTimerDisplayView.StateView.DeactivateLockTimer();
		_pomodoroTimerDisplayView.ActivateOpenButton();
		PomodoroTimerDisplayViewForMobile[] displayOnlyViews = _displayOnlyViews;
		for (int i = 0; i < displayOnlyViews.Length; i++)
		{
			displayOnlyViews[i].StateView.DeactivateLockTimer();
		}
	}

	private void SetupSetting()
	{
		_pomodoroTimerSettingView.Setup();
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerSettingView.OnClose, delegate
		{
			_onClose.OnNext((false, true));
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnUnpause, delegate
		{
			_onClose.OnNext((true, false));
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerSettingView.OnClickPlay, delegate
		{
			_onClose.OnNext((true, false));
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerSettingView.OnClickSlip, delegate
		{
			_onClose.OnNext((!_pomodoroService.CheckNextSkipCompleted(), false));
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerSettingView.OnClickReset, delegate
		{
			_onClose.OnNext((false, false));
		}).AddTo(this);
	}

	private void UpdateAdNoticeTextView()
	{
	}

	public void Activate()
	{
		UpdateAdNoticeTextView();
		IsActive = true;
		_pomodoroTimerSettingView.Activate();
	}

	public async UniTask Deactivate()
	{
		IsActive = false;
		await _pomodoroTimerSettingView.Deactivate();
	}
}
