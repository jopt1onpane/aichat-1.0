using System;
using UnityEngine;

namespace KanKikuchi.AudioManager;

public static class BGMSwitcher
{
	public static void FadeOut(string audioPath, float fadeOutDuration = 1f, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, Action callback = null)
	{
		if (!SingletonMonoBehaviour<BGMManager>.Instance.IsPlaying())
		{
			SingletonMonoBehaviour<BGMManager>.Instance.Play(audioPath, volumeRate, delay, pitch, isLoop);
			return;
		}
		SingletonMonoBehaviour<BGMManager>.Instance.FadeOut(fadeOutDuration, delegate
		{
			SingletonMonoBehaviour<BGMManager>.Instance.Play(audioPath, volumeRate, delay, pitch, isLoop);
			callback?.Invoke();
		});
	}

	public static void FadeIn(string audioPath, float fadeInDuration = 1f, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, Action callback = null)
	{
		SingletonMonoBehaviour<BGMManager>.Instance.Stop();
		SingletonMonoBehaviour<BGMManager>.Instance.Play(audioPath, volumeRate, delay, pitch, isLoop);
		SingletonMonoBehaviour<BGMManager>.Instance.FadeIn(audioPath, fadeInDuration, callback);
	}

	public static void FadeOutAndFadeIn(string audioPath, float fadeOutDuration = 1f, float fadeInDuration = 1f, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, Action callback = null)
	{
		if (!SingletonMonoBehaviour<BGMManager>.Instance.IsPlaying())
		{
			FadeIn(audioPath, fadeInDuration, volumeRate, delay, pitch, isLoop, callback);
			return;
		}
		SingletonMonoBehaviour<BGMManager>.Instance.FadeOut(fadeOutDuration, delegate
		{
			FadeIn(audioPath, fadeInDuration, volumeRate, delay, pitch, isLoop, callback);
		});
	}

	public static void CrossFade(string audioPath, float fadeDuration = 1f, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, Action callback = null)
	{
		if (SingletonMonoBehaviour<BGMManager>.Instance.GetCurrentAudioNames().Count >= SingletonMonoBehaviour<BGMManager>.Instance.AudioPlayerNum)
		{
			Debug.LogWarning("クロスフェードするにはAudio Player Numが足りません");
		}
		foreach (string currentAudioName in SingletonMonoBehaviour<BGMManager>.Instance.GetCurrentAudioNames())
		{
			SingletonMonoBehaviour<BGMManager>.Instance.FadeOut(currentAudioName, fadeDuration);
		}
		SingletonMonoBehaviour<BGMManager>.Instance.Play(audioPath, volumeRate, delay, pitch, isLoop, allowsDuplicate: true);
		SingletonMonoBehaviour<BGMManager>.Instance.FadeIn(audioPath, fadeDuration, callback);
	}
}
