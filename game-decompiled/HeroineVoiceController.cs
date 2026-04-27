using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using UnityEngine;

public class HeroineVoiceController
{
	private bool _isFinishedVoice = true;

	private CancellationTokenSource _voiceCancellationTokenSource;

	private Animator _heroineAnimater;

	private DateTime _lastVoiceDateTime = DateTime.MinValue;

	public bool IsFinishedVoice => _isFinishedVoice;

	public DateTime LastVoiceDateTime => _lastVoiceDateTime;

	public void Setup(Animator animator)
	{
		_heroineAnimater = animator;
	}

	public async UniTaskVoid PlayVoice(string voiceName, bool isMoveMouse, bool isStory = false)
	{
		if (!isStory && !_isFinishedVoice)
		{
			return;
		}
		_voiceCancellationTokenSource?.Cancel();
		_voiceCancellationTokenSource?.Dispose();
		_voiceCancellationTokenSource = new CancellationTokenSource();
		await UniTask.NextFrame(_voiceCancellationTokenSource.Token);
		if (isMoveMouse)
		{
			ChangeEnableHeroineTalkAnimation(isEnable: true);
		}
		try
		{
			SingletonMonoBehaviour<VoiceManager>.Instance.Stop();
			if (string.IsNullOrEmpty(voiceName))
			{
				return;
			}
			_isFinishedVoice = false;
			Action callback = delegate
			{
				ChangeEnableHeroineTalkAnimation(isEnable: false);
				_isFinishedVoice = true;
			};
			_lastVoiceDateTime = DateTime.Now;
			if (SingletonMonoBehaviour<VoiceManager>.Instance.Play(voiceName, 1f, 0f, 1f, isLoop: false, callback) == null)
			{
				Debug.LogError("voiceName:" + voiceName + " is not found");
				return;
			}
			await UniTask.WaitUntil(() => _isFinishedVoice, PlayerLoopTiming.Update, _voiceCancellationTokenSource.Token);
		}
		finally
		{
			_isFinishedVoice = true;
		}
	}

	public void CancelVoice()
	{
		SingletonMonoBehaviour<VoiceManager>.Instance.Stop();
		ChangeEnableHeroineTalkAnimation(isEnable: false);
		_voiceCancellationTokenSource?.Cancel();
		_voiceCancellationTokenSource?.Dispose();
		_voiceCancellationTokenSource = null;
	}

	public void ChangeEnableHeroineTalkAnimation(bool isEnable)
	{
		_heroineAnimater.SetBool(HeroineService.AnimationType.Enable_Talk.ToString(), isEnable);
	}

	public void EndNoVoiceTalk()
	{
		if (_isFinishedVoice)
		{
			ChangeEnableHeroineTalkAnimation(isEnable: false);
		}
	}

	public bool IsPlayingMouthTalkMotion()
	{
		if (_heroineAnimater.GetBool(HeroineService.AnimationType.Enable_Talk.ToString()))
		{
			return true;
		}
		return false;
	}
}
