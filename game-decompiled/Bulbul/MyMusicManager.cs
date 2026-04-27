using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using UnityEngine;

namespace Bulbul;

public class MyMusicManager : IDisposable
{
	private readonly List<GameAudioInfo> _playingList = new List<GameAudioInfo>();

	private List<GameAudioInfo> _randomPlayList = new List<GameAudioInfo>();

	private readonly List<GameAudioInfo> _checkRandomPlayList = new List<GameAudioInfo>();

	public IReadOnlyList<GameAudioInfo> PlayingList => _playingList;

	public GameAudioInfo PlayingMusic { get; private set; }

	public float Volume { get; private set; } = 1f;

	public bool IsRepeatOneMusic { get; private set; } = true;

	public bool IsRandom { get; private set; }

	public void Dispose()
	{
		ClearPlayingList();
	}

	private void ClearPlayingList()
	{
		foreach (GameAudioInfo playing in _playingList)
		{
			playing.UnloadAudioClip();
		}
		_playingList.Clear();
		_randomPlayList.Clear();
	}

	public void RemoveFromPlayingList(GameAudioInfo removeAudio)
	{
		if (removeAudio != null)
		{
			if (PlayingMusic == removeAudio)
			{
				PlayingMusic.UnloadAudioClip();
				SingletonMonoBehaviour<MusicManager>.Instance.Stop(PlayingMusic.AudioClipName);
				PlayingMusic = null;
			}
			_playingList.RemoveAll((GameAudioInfo x) => x == removeAudio);
		}
	}

	public void SetMusicItems(IEnumerable<GameAudioInfo> musicItems)
	{
		ClearPlayingList();
		_playingList.AddRange(musicItems.Take(100));
	}

	public void AddMusicItems(IEnumerable<GameAudioInfo> musicItems)
	{
		List<string> list = (from x in _playingList
			where x.PathType == AudioMode.LocalPc
			select x.LocalPath).ToList();
		foreach (GameAudioInfo musicItem in musicItems)
		{
			if (_playingList.Count >= 100)
			{
				break;
			}
			if (musicItem.PathType == AudioMode.LocalPc && (list.Contains(musicItem.LocalPath) || string.IsNullOrEmpty(musicItem.LocalPath)))
			{
				Debug.LogWarning(musicItem.LocalPath + " has already been added");
				continue;
			}
			_playingList.Add(musicItem);
			list.Add(musicItem.LocalPath);
		}
	}

	public async UniTask PlayNextMusic(int nextCount)
	{
		GameAudioInfo gameAudioInfo;
		AudioClip audioClip;
		if (!IsRandom)
		{
			(gameAudioInfo, audioClip) = await GetNextGameAudio(nextCount, _playingList);
		}
		else
		{
			(gameAudioInfo, audioClip) = await GetNextGameAudioRandom(nextCount);
		}
		if (gameAudioInfo == null || audioClip == null)
		{
			Debug.LogWarning("GameAudioInfo is null");
			return;
		}
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.Stop(PlayingMusic.AudioClipName);
			PlayingMusic = null;
		}
		audioClip.LoadAudioData();
		SingletonMonoBehaviour<MusicManager>.Instance.Play(audioClip, Volume, 0f, 1f, IsRepeatOneMusic, allowsDuplicate: true, "", delegate
		{
			PlayNextMusic(1).Forget();
		});
		PlayingMusic = gameAudioInfo;
	}

	private async UniTask<(GameAudioInfo, AudioClip)> GetNextGameAudioRandom(int nextCount)
	{
		nextCount = Mathf.Abs(nextCount);
		int num = _randomPlayList.IndexOf(PlayingMusic);
		int num2 = num + nextCount;
		if (!IsSameClips() || num < 0 || num2 < 0 || num2 >= _randomPlayList.Count)
		{
			_randomPlayList = _playingList.OrderBy((GameAudioInfo i) => Guid.NewGuid()).ToList();
			nextCount = 0;
		}
		return await GetNextGameAudio(nextCount, _randomPlayList);
		bool IsSameClips()
		{
			if (_playingList.Count != _randomPlayList.Count)
			{
				return false;
			}
			_checkRandomPlayList.Clear();
			_checkRandomPlayList.AddRange(_randomPlayList);
			foreach (GameAudioInfo playing in _playingList)
			{
				_checkRandomPlayList.Remove(playing);
			}
			return _checkRandomPlayList.Count == 0;
		}
	}

	private async UniTask<(GameAudioInfo, AudioClip)> GetNextGameAudio(int nextCount, List<GameAudioInfo> targetPlayList)
	{
		int sign = ((nextCount >= 0) ? 1 : (-1));
		int currentIndex = targetPlayList.IndexOf(PlayingMusic);
		currentIndex = ((currentIndex >= 0) ? currentIndex : 0);
		for (int i = 0; i < targetPlayList.Count; i++)
		{
			int index = (int)Mathf.Repeat(currentIndex + nextCount + sign * i, targetPlayList.Count);
			GameAudioInfo nextMusic = targetPlayList[index];
			if (nextMusic != null)
			{
				AudioClip audioClip = await nextMusic.GetAudioClip(CancellationToken.None);
				if (!(audioClip == null))
				{
					return (nextMusic, audioClip);
				}
			}
		}
		return (null, null);
	}

	public void Stop()
	{
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetAudioPlayerByName(PlayingMusic.AudioClipName)?.Stop();
		}
		PlayingMusic = null;
	}

	public void Pause()
	{
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetAudioPlayerByName(PlayingMusic.AudioClipName)?.Pause();
		}
	}

	public void UnPause()
	{
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.UnPause(PlayingMusic.AudioClipName);
		}
	}

	public void ChangeVolume(float vol)
	{
		Volume = Mathf.Clamp01(vol);
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetAudioPlayerByName(PlayingMusic.AudioClipName)?.ChangeVolumeRate(Volume);
		}
	}

	public void EnableRandomPlayback(bool enable)
	{
		IsRandom = enable;
		if (!IsRandom)
		{
			_randomPlayList.Clear();
		}
	}

	public void SetRepeat(bool isrepeat)
	{
		IsRepeatOneMusic = isrepeat;
		if (PlayingMusic != null && !string.IsNullOrEmpty(PlayingMusic.AudioClipName))
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetAudioPlayerByName(PlayingMusic.AudioClipName)?.SetLoop(isrepeat);
		}
	}
}
