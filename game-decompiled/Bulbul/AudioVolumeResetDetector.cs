using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using KanKikuchi.AudioManager;
using UnityEngine;
using VContainer.Unity;

namespace Bulbul;

public class AudioVolumeResetDetector : IAsyncStartable
{
	private readonly AudioMixerGroupContainer audioMixer;

	public AudioVolumeResetDetector(AudioMixerGroupContainer audioMixer)
	{
		this.audioMixer = audioMixer;
	}

	public async UniTask StartAsync(CancellationToken cancellation)
	{
		InitializeDetector();
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
		cancellation.Register(delegate
		{
			AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
		});
		IUniTaskAsyncEnumerator<AsyncUnit> asyncEnumerator = UniTaskAsyncEnumerable.EveryUpdate().GetAsyncEnumerator();
		object obj = null;
		try
		{
			while (await asyncEnumerator.MoveNextAsync())
			{
				_ = asyncEnumerator.Current;
				if (IsDetectReset())
				{
					audioMixer.ManualUpdateAllVolume();
					SingletonMonoBehaviour<MusicManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<AmbientBGMManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<AmbientSEManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<ScenarioMusicManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<VoiceManager>.Instance.OnAudioReloaded();
					SingletonMonoBehaviour<SEManager>.Instance.OnAudioReloaded();
					InitializeDetector();
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (asyncEnumerator != null)
		{
			await asyncEnumerator.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
	}

	private void OnAudioConfigurationChanged(bool deviceChanged)
	{
		audioMixer.ManualUpdateAllVolume();
		InitializeDetector();
	}

	private void InitializeDetector()
	{
		audioMixer.AudioMixer.SetFloat("VolumeDetector", -80f);
	}

	private bool IsDetectReset()
	{
		if (!audioMixer.AudioMixer.GetFloat("VolumeDetector", out var value))
		{
			return false;
		}
		return value > 0f;
	}
}
