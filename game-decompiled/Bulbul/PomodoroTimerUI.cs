using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class PomodoroTimerUI : MonoBehaviour, IPomodoroTimerUI
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[SerializeField]
	[Header("作業時間分数")]
	private TMP_InputField _workTimeMaxMinutesText;

	[SerializeField]
	[Header("休憩時間分数")]
	private TMP_InputField _breakTimeMaxMinutesText;

	[SerializeField]
	[Header("ループ回数")]
	private TMP_InputField _loopCountMaxText;

	[SerializeField]
	[Header("メーター画像\u3000時間によって上から表示するImage")]
	private Image _meterOverWriteImage;

	[SerializeField]
	[Header("残りループ回数テキスト")]
	private TextMeshProUGUI _remainLoopCountText;

	[SerializeField]
	[Header("時間用テキスト")]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	[Header("作業時間\u3000増加ボタン")]
	private LongPressButton _addWorkTimeButton;

	[SerializeField]
	[Header("作業時間\u3000減少ボタン")]
	private LongPressButton _subtractWorkTimeButton;

	[SerializeField]
	[Header("休憩時間\u3000増加ボタン")]
	private LongPressButton _addBreakTimeButton;

	[SerializeField]
	[Header("休憩時間\u3000減少ボタン")]
	private LongPressButton _subtractBreakTimeButton;

	[SerializeField]
	[Header("ループ回数\u3000増加ボタン")]
	private LongPressButton _addLoopCountButton;

	[SerializeField]
	[Header("ループ回数\u3000減少ボタン")]
	private LongPressButton _subtractLoopButtonButton;

	[SerializeField]
	[Header("リセットボタン")]
	private Button _resetButton;

	[SerializeField]
	[Header("ポモドーロ開始ボタン")]
	private Button _startPomodoroButton;

	[SerializeField]
	[Header("ポモドーロ再生、一時停止ボタン")]
	private Button _playOrPausePomodoroButton;

	[SerializeField]
	[Header("スキップボタン")]
	private Button _skipPomodoroButton;

	private Subject<Unit> _onClickButtonIncrementWorkTime = new Subject<Unit>();

	private Subject<Unit> _onClickButtonDecrementWorkTime = new Subject<Unit>();

	private Subject<int> _onEndEditWorkTimeMaxMinutesText = new Subject<int>();

	private Subject<Unit> _onClickButtonIncrementBreakTime = new Subject<Unit>();

	private Subject<Unit> _onClickButtonDecrementBreakTime = new Subject<Unit>();

	private Subject<int> _onEndEditBreakTimeMaxMinutesText = new Subject<int>();

	private Subject<Unit> _onClickButtonIncrementLoopCount = new Subject<Unit>();

	private Subject<Unit> _onClickButtonDecrementLoopCount = new Subject<Unit>();

	private Subject<int> _onEndEditLoopCountText = new Subject<int>();

	private Subject<Unit> _onClickButtonReset = new Subject<Unit>();

	private Subject<Unit> _onClickButtonStartPomodoro = new Subject<Unit>();

	private Subject<Unit> _onClickButtonPlayOrPausePomodoro = new Subject<Unit>();

	private Subject<Unit> _onClickButtonSkipPomodoro = new Subject<Unit>();

	private IPomodoroTimerStateView _pomodoroTimerStateView;

	public Observable<Unit> OnClickButtonIncrementWorkTime => _onClickButtonIncrementWorkTime;

	public Observable<Unit> OnClickButtonDecrementWorkTime => _onClickButtonDecrementWorkTime;

	public Observable<int> OnEndEditWorkTimeMaxMinutesText => _onEndEditWorkTimeMaxMinutesText;

	public Observable<Unit> OnClickButtonIncrementBreakTime => _onClickButtonIncrementBreakTime;

	public Observable<Unit> OnClickButtonDecrementBreakTime => _onClickButtonDecrementBreakTime;

	public Observable<int> OnEndEditBreakTimeMaxMinutesText => _onEndEditBreakTimeMaxMinutesText;

	public Observable<Unit> OnClickButtonIncrementLoopCount => _onClickButtonIncrementLoopCount;

	public Observable<Unit> OnClickButtonDecrementLoopCount => _onClickButtonDecrementLoopCount;

	public Observable<int> OnEndEditLoopCountText => _onEndEditLoopCountText;

	public Observable<Unit> OnClickButtonReset => _onClickButtonReset;

	public Observable<Unit> OnClickButtonStartPomodoro => _onClickButtonStartPomodoro;

	public Observable<Unit> OnClickButtonPlayOrPausePomodoro => _onClickButtonPlayOrPausePomodoro;

	public Observable<Unit> OnClickButtonSkipPomodoro => _onClickButtonSkipPomodoro;

	public void Setup()
	{
		if (_pomodoroTimerStateView == null)
		{
			_pomodoroTimerStateView = GetComponentInChildren<IPomodoroTimerStateView>(includeInactive: true);
		}
		_pomodoroTimerStateView.Setup();
		if (_addWorkTimeButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(_addWorkTimeButton.OnLongPress, delegate
			{
				_onClickButtonIncrementWorkTime.OnNext(Unit.Default);
			}).AddTo(this);
		}
		if (_subtractWorkTimeButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(_subtractWorkTimeButton.OnLongPress, delegate
			{
				_onClickButtonDecrementWorkTime.OnNext(Unit.Default);
			}).AddTo(this);
		}
		_workTimeMaxMinutesText.OnEndEditAsObservable().Subscribe(delegate(string text)
		{
			if (int.TryParse(text, out var result))
			{
				_onEndEditWorkTimeMaxMinutesText.OnNext(result);
				_workTimeMaxMinutesText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value.ToString());
			}
			else
			{
				_workTimeMaxMinutesText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value.ToString());
			}
		}).AddTo(this);
		_workTimeMaxMinutesText.DisableIME();
		SaveDataManager.Instance.PomodoroData.WorkMinutes.Subscribe(delegate(int minutes)
		{
			_workTimeMaxMinutesText.SetTextWithoutNotify(minutes.ToString());
		}).AddTo(this);
		if (_addBreakTimeButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(_addBreakTimeButton.OnLongPress, delegate
			{
				_onClickButtonIncrementBreakTime.OnNext(Unit.Default);
			}).AddTo(this);
		}
		if (_subtractBreakTimeButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(_subtractBreakTimeButton.OnLongPress, delegate
			{
				_onClickButtonDecrementBreakTime.OnNext(Unit.Default);
			}).AddTo(this);
		}
		_breakTimeMaxMinutesText.OnEndEditAsObservable().Subscribe(delegate(string text)
		{
			if (int.TryParse(text, out var result))
			{
				_onEndEditBreakTimeMaxMinutesText.OnNext(result);
				_breakTimeMaxMinutesText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value.ToString());
			}
			else
			{
				_breakTimeMaxMinutesText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value.ToString());
			}
		}).AddTo(this);
		_breakTimeMaxMinutesText.DisableIME();
		SaveDataManager.Instance.PomodoroData.BreakMinutes.Subscribe(delegate(int breakMinutes)
		{
			_breakTimeMaxMinutesText.SetTextWithoutNotify(breakMinutes.ToString());
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_addLoopCountButton.OnLongPress, delegate
		{
			_onClickButtonIncrementLoopCount.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_subtractLoopButtonButton.OnLongPress, delegate
		{
			_onClickButtonDecrementLoopCount.OnNext(Unit.Default);
		}).AddTo(this);
		_loopCountMaxText.OnEndEditAsObservable().Subscribe(delegate(string text)
		{
			if (int.TryParse(text, out var result))
			{
				_onEndEditLoopCountText.OnNext(result);
				_loopCountMaxText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.LoopCount.Value.ToString());
			}
			else
			{
				_loopCountMaxText.SetTextWithoutNotify(SaveDataManager.Instance.PomodoroData.LoopCount.Value.ToString());
			}
		}).AddTo(this);
		_loopCountMaxText.DisableIME();
		SaveDataManager.Instance.PomodoroData.LoopCount.Subscribe(delegate(int loopCount)
		{
			_loopCountMaxText.SetTextWithoutNotify(loopCount.ToString());
		}).AddTo(this);
		_pomodoroService.CurrentLoopCount.Subscribe(delegate(int currentLoopCount)
		{
			_remainLoopCountText.text = currentLoopCount + "/" + SaveDataManager.Instance.PomodoroData.LoopCount;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_resetButton.OnClickAsObservable(), delegate
		{
			_onClickButtonReset.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_startPomodoroButton.OnClickAsObservable(), delegate
		{
			_onClickButtonStartPomodoro.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnStartPomodoro, delegate
		{
			_pomodoroTimerStateView.SwitchStartTimer();
			TimeSpan timeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value);
			_timeText.text = timeSpan.ToString("hh\\:mm\\:ss");
			_remainLoopCountText.text = "1/" + SaveDataManager.Instance.PomodoroData.LoopCount.Value;
		}).AddTo(this);
		_pomodoroService.OnPlayPomodoro.Subscribe(delegate(PomodoroService.PomodoroType type)
		{
			_pomodoroTimerStateView.SwitchPlayTimer(type, delegate(PomodoroService.PomodoroType pomodoroType)
			{
				switch (pomodoroType)
				{
				case PomodoroService.PomodoroType.Work:
				{
					TimeSpan timeSpan2 = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value);
					_timeText.text = timeSpan2.ToString("hh\\:mm\\:ss");
					break;
				}
				case PomodoroService.PomodoroType.Break:
				{
					TimeSpan timeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value);
					_timeText.text = timeSpan.ToString("hh\\:mm\\:ss");
					break;
				}
				}
				_remainLoopCountText.text = _pomodoroService.CurrentLoopCount.CurrentValue + "/" + SaveDataManager.Instance.PomodoroData.LoopCount.Value;
			});
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_playOrPausePomodoroButton.OnClickAsObservable(), delegate
		{
			_onClickButtonPlayOrPausePomodoro.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_skipPomodoroButton.OnClickAsObservable(), delegate
		{
			_onClickButtonSkipPomodoro.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
		{
			_pomodoroTimerStateView.SwitchCompleteTimer(delegate
			{
				_pomodoroService.ActivateInput();
			});
		}).AddTo(this);
		_pomodoroService.OnUpdatePomodoro.Subscribe(delegate((PomodoroService.PomodoroType, TimeSpan, TimeSpan) info)
		{
			if (!_pomodoroTimerStateView.IsTransitioning)
			{
				var (pomodoroType, timeSpan, timeSpan2) = info;
				switch (pomodoroType)
				{
				case PomodoroService.PomodoroType.Work:
					_meterOverWriteImage.fillAmount = 1f - (float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds;
					break;
				case PomodoroService.PomodoroType.Break:
					_meterOverWriteImage.fillAmount = (float)timeSpan.TotalSeconds / (float)timeSpan2.TotalSeconds;
					break;
				}
				_timeText.text = timeSpan.ToString("hh\\:mm\\:ss");
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnPause, delegate
		{
			_pomodoroTimerStateView.SwitchPauseTimer();
		}).AddTo(this);
		_pomodoroService.OnUnpause.Subscribe(delegate(PomodoroService.PomodoroType pomodoroType)
		{
			_pomodoroTimerStateView.SwitchUnPauseTimer(pomodoroType);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			_pomodoroTimerStateView.ActivateLockTimer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			_pomodoroTimerStateView.DeactivateLockTimer();
		}).AddTo(this);
	}
}
