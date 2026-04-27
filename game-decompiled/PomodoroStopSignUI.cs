using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

public class PomodoroStopSignUI : MonoBehaviour, IPomodoroStopSignUI
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private GameObject _pauseTextObj;

	private CancellationTokenSource _shakeCts;

	public void Setup()
	{
		_canvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnPause, delegate
		{
			if (!_scenarioReader.IsPlayingMiddleTalk())
			{
				_canvasGroup.DOFade(1f, 0.2f);
				StartShakeAnimation().Forget();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnUnpause, delegate
		{
			_canvasGroup.DOFade(0f, 0.2f);
			StopShakeAnimation();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
		{
			_canvasGroup.DOFade(0f, 0.2f);
			StopShakeAnimation();
		}).AddTo(this);
	}

	private async UniTaskVoid StartShakeAnimation()
	{
		StopShakeAnimation();
		_shakeCts = new CancellationTokenSource();
		try
		{
			while (!_shakeCts.Token.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(2.1500000953674316), ignoreTimeScale: false, PlayerLoopTiming.Update, _shakeCts.Token);
				((RectTransform)_pauseTextObj.transform).DOPunchAnchorPos(new Vector2(10f, 0f), 0.5f);
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private void StopShakeAnimation()
	{
		_shakeCts?.Cancel();
		_shakeCts?.Dispose();
		_shakeCts = null;
		_pauseTextObj.transform.DOKill();
	}

	private void OnDestroy()
	{
		StopShakeAnimation();
	}
}
