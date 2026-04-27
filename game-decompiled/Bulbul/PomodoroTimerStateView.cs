using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class PomodoroTimerStateView : MonoBehaviour, IPomodoroTimerStateView
{
	private static readonly int PomodoroStartAndEndAnimHash = Animator.StringToHash("PomodoroStartAndFinish");

	[SerializeField]
	[Header("設定UI親オブジェクト")]
	private GameObject _settingParentObject;

	[SerializeField]
	[Header("ポモドーロ親オブジェクト")]
	private GameObject _usingTimerParentObject;

	[SerializeField]
	[Header("残りループ回数テキスト")]
	private TextMeshProUGUI _remainLoopCountText;

	[SerializeField]
	[Header("一時停止中に上から表示するImage")]
	private Image _pauseOverWriteImage;

	[SerializeField]
	[Header("一時停止中の時間テキストColor")]
	private Color _pauseTimeTextColor;

	[SerializeField]
	[Header("休憩中に上から表示するImage")]
	private Image _restOverWriteImage;

	[SerializeField]
	[Header("メーター画像\u3000時間によって上から表示するImage")]
	private Image _meterOverWriteImage;

	[SerializeField]
	[Header("時間用テキスト")]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	[Header("ポモドーロタイプテキスト(WorkかRestか)")]
	private TextLocalizationBehaviour _pomodoroTypeText;

	[SerializeField]
	[Header("ポモドーロ再生またはポーズ用のImage")]
	private Image _pomodoroPlayOrPauseButtonImage;

	[SerializeField]
	private Sprite _playButtonSprite;

	[SerializeField]
	private Sprite _pauseButtonSprite;

	[SerializeField]
	[Header("スキップボタンオブジェクト")]
	private GameObject _skipButtonObject;

	[SerializeField]
	private float _fadeTweenSecond = 0.25f;

	[SerializeField]
	private float _textCollorTweenSecond = 0.25f;

	[SerializeField]
	private CanvasGroup _settingCanvas;

	[SerializeField]
	private CanvasGroup _usingTimerCanvas;

	[SerializeField]
	[Header("ポモドーロのロックUI")]
	private LockUI _lockUI;

	[SerializeField]
	[Header("ポモドーロ開始、終了アニメーション")]
	private Animator _pomodoroAnimator;

	private Color _defaultTimeTextColor;

	public bool IsTransitioning { get; private set; }

	public void Setup()
	{
		_meterOverWriteImage.fillAmount = 0f;
		_restOverWriteImage.DOFade(0f, 0f);
		_pauseOverWriteImage.DOFade(0f, 0f);
		_settingParentObject.SetActive(value: true);
		_settingCanvas.DOFade(1f, 0f);
		_usingTimerParentObject.SetActive(value: false);
		_usingTimerCanvas.DOFade(0f, 0f);
		_defaultTimeTextColor = _timeText.color;
		_lockUI.Setup();
		_pomodoroAnimator.writeDefaultValuesOnDisable = true;
	}

	public void SwitchStartTimer()
	{
		_settingCanvas.DOFade(0f, _fadeTweenSecond).OnComplete(delegate
		{
			_settingParentObject.SetActive(value: false);
			_usingTimerParentObject.SetActive(value: true);
		});
		_remainLoopCountText.enabled = true;
		_pauseOverWriteImage.DOFade(0f, _fadeTweenSecond);
		OnPlayPomodoroTimer(PomodoroService.PomodoroType.Work);
		_timeText.color = _defaultTimeTextColor;
		_pomodoroPlayOrPauseButtonImage.sprite = _pauseButtonSprite;
		_skipButtonObject.gameObject.SetActive(value: true);
	}

	public void SwitchPlayTimer(PomodoroService.PomodoroType pomodoroType, Action<PomodoroService.PomodoroType> completedUsingTimerCanvasFade)
	{
		IsTransitioning = true;
		_usingTimerCanvas.DOFade(0f, _fadeTweenSecond).OnComplete(delegate
		{
			switch (pomodoroType)
			{
			case PomodoroService.PomodoroType.Work:
				_meterOverWriteImage.fillAmount = 0f;
				_meterOverWriteImage.fillClockwise = true;
				break;
			case PomodoroService.PomodoroType.Break:
				_meterOverWriteImage.fillAmount = 1f;
				_meterOverWriteImage.fillClockwise = false;
				break;
			}
			OnPlayPomodoroTimer(pomodoroType);
			_usingTimerCanvas.DOFade(1f, _fadeTweenSecond).OnComplete(delegate
			{
				IsTransitioning = false;
			});
			completedUsingTimerCanvasFade?.Invoke(pomodoroType);
		});
		_pomodoroAnimator.Play(PomodoroStartAndEndAnimHash, 0, 0f);
	}

	public void SwitchCompleteTimer(Action completedUsingTimerCanvasFade)
	{
		_usingTimerCanvas.DOFade(0f, _fadeTweenSecond).OnComplete(delegate
		{
			_usingTimerParentObject.SetActive(value: false);
			_settingParentObject.SetActive(value: true);
			_settingCanvas.DOFade(1f, _fadeTweenSecond);
			_meterOverWriteImage.fillAmount = 0f;
			_pomodoroPlayOrPauseButtonImage.sprite = _playButtonSprite;
			_remainLoopCountText.enabled = false;
			completedUsingTimerCanvasFade?.Invoke();
		});
		_pomodoroAnimator.Play(PomodoroStartAndEndAnimHash, 0, 0f);
	}

	public void SwitchPauseTimer()
	{
		_pauseOverWriteImage.DOFade(1f, _fadeTweenSecond);
		_pomodoroTypeText.Set("ui_pomodoro_pause_title");
		_timeText.DOColor(_pauseTimeTextColor, _textCollorTweenSecond);
		_pomodoroPlayOrPauseButtonImage.sprite = _playButtonSprite;
		_skipButtonObject.gameObject.SetActive(value: false);
	}

	public void SwitchUnPauseTimer(PomodoroService.PomodoroType pomodoroType)
	{
		_pauseOverWriteImage.DOFade(0f, _fadeTweenSecond);
		OnPlayPomodoroTimer(pomodoroType);
		_timeText.color = _defaultTimeTextColor;
		_pomodoroPlayOrPauseButtonImage.sprite = _pauseButtonSprite;
		_skipButtonObject.gameObject.SetActive(value: true);
	}

	public void ActivateLockTimer()
	{
		_lockUI.Activate();
	}

	public void DeactivateLockTimer()
	{
		_lockUI.Deactivate();
	}

	private void OnPlayPomodoroTimer(PomodoroService.PomodoroType pomodoroType)
	{
		switch (pomodoroType)
		{
		case PomodoroService.PomodoroType.Work:
			_restOverWriteImage.DOFade(0f, _fadeTweenSecond);
			_pomodoroTypeText.Set("ui_pomodoro_work_title");
			break;
		case PomodoroService.PomodoroType.Break:
			_restOverWriteImage.DOFade(1f, _fadeTweenSecond);
			_pomodoroTypeText.Set("ui_pomodoro_break_title");
			break;
		}
		_pomodoroPlayOrPauseButtonImage.sprite = _pauseButtonSprite;
	}
}
