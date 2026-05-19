using System;
using R3;
using R3.Triggers;
using UnityEngine;

namespace KanKikuchi.AudioManager;

public class AudioPlayer
{
	public enum State
	{
		Wait,
		Delay,
		Playing,
		Pause,
		Fading
	}

	private readonly AudioSource _audioSource;

	private Action _callback;

	private State _currentState;

	private float _baseVolume;

	private float _volumeRate;

	private double _initialDelay;

	private double _currentDelay;

	private float _fadeProgress;

	private float _fadeDuration;

	private float _fadeFrom;

	private float _fadeTo;

	private Action _fadeCallback;

	private int _stopFrameCount;

	private int _stopFrameLimit;

	private int _lastPlayTimeSamples;

	private bool audioClipBroke;

	public string Key;

	public AudioSource AudioSource => _audioSource;

	public float PlayedTime => _audioSource.time;

	public string CurrentAudioName
	{
		get
		{
			if (!(_audioSource.clip == null))
			{
				return _audioSource.clip.name;
			}
			return "";
		}
	}

	public State CurrentState => _currentState;

	public float CurrentVolume => _baseVolume * _volumeRate;

	public double ElapsedDelay => _initialDelay - _currentDelay;

	public AudioPlayer(AudioSource audioSource)
	{
		_audioSource = audioSource;
		_audioSource.playOnAwake = false;
		OnChangeState();
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
		_audioSource.OnDestroyAsObservable().Subscribe(delegate
		{
			AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
		});
	}

	public void Update()
	{
		if (_currentState == State.Playing)
		{
			if (!_audioSource.isPlaying && Mathf.Approximately(_audioSource.time, 0f))
			{
				if (_stopFrameLimit > 0)
				{
					if (!_audioSource.clip || Mathf.Approximately(_audioSource.clip.length, 0f))
					{
						audioClipBroke = true;
						return;
					}
					_stopFrameCount++;
					if (!audioClipBroke && _stopFrameCount >= _stopFrameLimit)
					{
						Finish();
					}
					else if (audioClipBroke && (bool)_audioSource.clip && _audioSource.clip.length > 0f)
					{
						_audioSource.timeSamples = _lastPlayTimeSamples;
						_audioSource.Play();
						_stopFrameCount = 0;
						audioClipBroke = false;
					}
				}
				else
				{
					Finish();
				}
			}
			else if (_audioSource.isPlaying && (bool)_audioSource.clip && _audioSource.clip.length > 0f)
			{
				_lastPlayTimeSamples = _audioSource.timeSamples;
				_stopFrameCount = 0;
				audioClipBroke = false;
			}
		}
		else if (_currentState == State.Delay)
		{
			Delay();
		}
		else if (_currentState == State.Fading)
		{
			Fade();
		}
	}

	private void OnAudioConfigurationChanged(bool deviceChanged)
	{
		OnAudioReloaded();
	}

	public void OnAudioReloaded()
	{
		if (_currentState == State.Playing && !_audioSource.isPlaying)
		{
			_audioSource.timeSamples = _lastPlayTimeSamples;
			_audioSource.Play();
		}
	}

	private void Delay()
	{
		_currentDelay -= Time.deltaTime;
		if (!(_currentDelay > 0.0))
		{
			if (_fadeDuration > 0f)
			{
				ChangeCurrentState(State.Fading);
				Update();
			}
			else
			{
				ChangeCurrentState(State.Playing);
			}
		}
	}

	private void Fade()
	{
		_fadeProgress += Time.deltaTime;
		float num = Mathf.Min(_fadeProgress / _fadeDuration, 1f);
		_volumeRate = _fadeFrom * (1f - num) + _fadeTo * num;
		_audioSource.volume = GetVolume();
		if (!(num < 1f))
		{
			if (_fadeTo <= 0f)
			{
				ChangeCurrentState(State.Wait);
			}
			else
			{
				ChangeCurrentState(State.Playing);
			}
			_fadeCallback?.Invoke();
			_initialDelay = 0.0;
			_currentDelay = 0.0;
			_fadeDuration = 0f;
		}
	}

	public void ChangeVolume(float baseVolume)
	{
		_baseVolume = baseVolume;
		_audioSource.volume = GetVolume();
	}

	public void ChangeVolumeRate(float volumeRate)
	{
		_volumeRate = volumeRate;
		_audioSource.volume = GetVolume();
	}

	private float GetVolume()
	{
		return _baseVolume * _volumeRate;
	}

	public void SetLoop(bool isLoop)
	{
		_audioSource.loop = isLoop;
	}

	public void Play(AudioClip audioClip, float baseVolume, float volumeRate, double delay, float pitch, bool isLoop, int finishWaitFrameCount = 0, Action callback = null)
	{
		if (_currentState != State.Wait)
		{
			Stop();
		}
		_audioSource.Stop();
		_audioSource.time = 0f;
		_volumeRate = volumeRate;
		ChangeVolume(baseVolume);
		_initialDelay = delay;
		_currentDelay = _initialDelay;
		_stopFrameLimit = finishWaitFrameCount;
		_stopFrameCount = 0;
		_lastPlayTimeSamples = 0;
		audioClipBroke = false;
		_audioSource.pitch = pitch;
		_audioSource.loop = isLoop;
		_callback = callback;
		_audioSource.clip = audioClip;
		if (audioClip != null)
		{
			audioClip.LoadAudioData();
		}
		ChangeCurrentState((_currentDelay > 0.0) ? State.Delay : State.Playing);
		if (_currentState == State.Playing)
		{
			_audioSource.Play();
		}
		if (_currentState == State.Delay)
		{
			_audioSource.PlayScheduled(AudioSettings.dspTime + delay);
		}
		if (!_audioSource.loop && _currentState == State.Pause)
		{
			Pause();
		}
	}

	public void Stop(string audioName)
	{
		if (audioName == CurrentAudioName)
		{
			Stop();
		}
	}

	public void Stop()
	{
		_callback = null;
		Finish();
	}

	private void Finish()
	{
		ChangeCurrentState(State.Wait);
		_audioSource.Stop();
		_audioSource.clip = null;
		_initialDelay = 0.0;
		_currentDelay = 0.0;
		_fadeDuration = 0f;
		_callback?.Invoke();
	}

	public void Pause(string audioName)
	{
		if (audioName == CurrentAudioName)
		{
			Pause();
		}
	}

	public void Pause()
	{
		if (_currentState == State.Playing || _currentState == State.Fading)
		{
			_audioSource.Pause();
		}
		ChangeCurrentState(State.Pause);
	}

	public void UnPause(string audioName)
	{
		if (audioName == CurrentAudioName)
		{
			UnPause();
		}
	}

	public void UnPause()
	{
		if (_currentState == State.Pause)
		{
			if (_audioSource.clip == null)
			{
				ChangeCurrentState(State.Wait);
				return;
			}
			if (_currentDelay > 0.0)
			{
				ChangeCurrentState(State.Delay);
				return;
			}
			_audioSource.UnPause();
			ChangeCurrentState((_fadeDuration > 0f) ? State.Fading : State.Playing);
		}
	}

	public void Fade(string audioName, float duration, float from, float to, Action callback = null)
	{
		if (audioName == CurrentAudioName)
		{
			Fade(duration, from, to, callback);
		}
	}

	public void Fade(float duration, float from, float to, Action callback = null)
	{
		if (_currentState == State.Playing || _currentState == State.Delay || _currentState == State.Fading)
		{
			_fadeProgress = 0f;
			_fadeDuration = duration;
			_fadeFrom = from;
			_fadeTo = to;
			_fadeCallback = callback;
			if (_currentState == State.Playing)
			{
				ChangeCurrentState(State.Fading);
			}
			if (_currentState == State.Fading)
			{
				Update();
			}
		}
	}

	private void ChangeCurrentState(State sta)
	{
		bool num = _currentState != sta;
		_currentState = sta;
		if (num)
		{
			OnChangeState();
		}
	}

	protected virtual void OnChangeState()
	{
	}
}
