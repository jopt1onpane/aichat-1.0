using UnityEngine;

namespace KanKikuchi.AudioManager;

public abstract class AudioAssistant : MonoBehaviour
{
	[SerializeField]
	protected AudioClip _audioClip;

	[SerializeField]
	protected bool _isAutoPlay;

	[SerializeField]
	protected float _volumeRate = 1f;

	[SerializeField]
	protected float _delay;

	[SerializeField]
	protected float _pitch = 1f;

	[SerializeField]
	protected float _fadeInDuration;

	public AudioClip AudioClip
	{
		get
		{
			return _audioClip;
		}
		set
		{
			_audioClip = value;
		}
	}

	public bool IsAutoPlay
	{
		get
		{
			return _isAutoPlay;
		}
		set
		{
			_isAutoPlay = value;
		}
	}

	public float VolumeRate
	{
		get
		{
			return _volumeRate;
		}
		set
		{
			_volumeRate = value;
		}
	}

	public float Delay
	{
		get
		{
			return _delay;
		}
		set
		{
			_delay = value;
		}
	}

	public float Pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = value;
		}
	}

	public float FadeInDuration
	{
		get
		{
			return _fadeInDuration;
		}
		set
		{
			_fadeInDuration = value;
		}
	}

	protected virtual void Start()
	{
		if (_isAutoPlay)
		{
			Play();
		}
	}

	public abstract void Play();
}
