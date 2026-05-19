using UnityEngine;
using UnityEngine.SceneManagement;

namespace KanKikuchi.AudioManager;

public class BGMAssistant : AudioAssistant
{
	[SerializeField]
	private float _fadeOutDuration;

	[SerializeField]
	private bool _isLoop = true;

	[SerializeField]
	private bool _isAutoStop = true;

	public float FadeOutDuration
	{
		get
		{
			return _fadeOutDuration;
		}
		set
		{
			_fadeOutDuration = value;
		}
	}

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

	public bool IsAutoStop
	{
		get
		{
			return _isAutoStop;
		}
		set
		{
			_isAutoStop = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_isAutoStop)
		{
			SceneManager.sceneUnloaded += OnUnloadedScene;
		}
	}

	private void OnUnloadedScene(Scene scene)
	{
		SceneManager.sceneUnloaded -= OnUnloadedScene;
		if (_fadeOutDuration > 0f)
		{
			SingletonMonoBehaviour<BGMManager>.Instance.FadeOut(_fadeOutDuration);
		}
		else
		{
			SingletonMonoBehaviour<BGMManager>.Instance.Stop();
		}
	}

	public override void Play()
	{
		if (_audioClip == null)
		{
			Debug.LogWarning(base.gameObject.name + "のBGMAssistantにAudioClipが設定されていません");
			return;
		}
		SingletonMonoBehaviour<BGMManager>.Instance.Play(_audioClip, _volumeRate, _delay, _pitch, _isLoop, _fadeInDuration > 0f);
		if (_fadeInDuration > 0f)
		{
			SingletonMonoBehaviour<BGMManager>.Instance.FadeIn(_audioClip.name, _fadeInDuration);
		}
	}
}
