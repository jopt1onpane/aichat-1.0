using System;
using UnityEngine;

namespace KanKikuchi.AudioManager;

public class SEAssistant : AudioAssistant
{
	[SerializeField]
	private bool _isLoop;

	public bool IsLoop
	{
		get
		{
			return _isLoop;
		}
		set
		{
			_isLoop = value;
		}
	}

	public override void Play()
	{
		Play(null);
	}

	public void Play(Action callback)
	{
		if (_audioClip == null)
		{
			Debug.LogWarning(base.gameObject.name + "のSEAssistantにAudioClipが設定されていません");
			callback?.Invoke();
			return;
		}
		SingletonMonoBehaviour<SEManager>.Instance.Play(_audioClip, _volumeRate, _delay, _pitch, _isLoop, callback);
		if (_fadeInDuration > 0f)
		{
			SingletonMonoBehaviour<SEManager>.Instance.FadeIn(_audioClip.name, _fadeInDuration);
		}
	}
}
