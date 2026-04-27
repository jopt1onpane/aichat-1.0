using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using NestopiSystem;
using ObservableCollections;
using R3;
using UnityEngine;
using VContainer;

public class MusicService : IDisposable
{
	[Inject]
	private AudioMixerGroupContainer _audioMixerGroup;

	private readonly List<GameAudioInfo> _allMusicList = new List<GameAudioInfo>();

	private List<GameAudioInfo> shuffleList = new List<GameAudioInfo>();

	private readonly AudioMixerGroupContainer audioMixerGroupContainer;

	private readonly Subject<MusicChangeKind> onChangeMusic = new Subject<MusicChangeKind>();

	private CancellationTokenSource audioLoadCancellation = new CancellationTokenSource();

	private readonly Subject<GameAudioInfo> onPlayMusic = new Subject<GameAudioInfo>();

	private readonly Subject<GameAudioInfo> onPauseMusic = new Subject<GameAudioInfo>();

	private readonly Subject<Unit> onEnableShuffle = new Subject<Unit>();

	private readonly Subject<Unit> onDisableShuffle = new Subject<Unit>();

	private readonly Subject<GameAudioInfo> onCompleteImportMusic = new Subject<GameAudioInfo>();

	private readonly Subject<GameAudioInfo> onRemoveLocalMusic = new Subject<GameAudioInfo>();

	private AudioTag favoritePrevTags;

	private DisposableBag disposable;

	public bool IsPause;

	public IReadOnlyList<GameAudioInfo> AllMusicList => _allMusicList;

	public ObservableList<GameAudioInfo> CurrentPlayList { get; private set; } = new ObservableList<GameAudioInfo>();

	public GameAudioInfo PlayingMusic { get; private set; }

	public bool IsRepeatOneMusic { get; private set; }

	public bool IsShuffle { get; private set; }

	public bool IsPlayListDirtyForLocalImport { get; private set; }

	public bool IsPlayListDirtyForLocalRemove { get; private set; }

	public Observable<MusicChangeKind> OnChangeMusic => onChangeMusic;

	public Observable<GameAudioInfo> OnPlayMusic => onPlayMusic;

	public Observable<GameAudioInfo> OnPauseMusic => onPauseMusic;

	public Observable<Unit> OnEnableShuffle => onEnableShuffle;

	public Observable<Unit> OnDisableShuffle => onDisableShuffle;

	public Observable<GameAudioInfo> OnCompleteImportMusic => onCompleteImportMusic;

	public Observable<GameAudioInfo> OnRemoveLocalMusic => onRemoveLocalMusic;

	public MusicService(AudioMixerGroupContainer audioMixerGroupContainer)
	{
		this.audioMixerGroupContainer = audioMixerGroupContainer;
	}

	public void Load(IReadOnlyCollection<GameAudioInfo> musicItems)
	{
		ClearPlayingList();
		_allMusicList.AddRange(musicItems.Where((GameAudioInfo m) => m.IsUnlocked));
		shuffleList = _allMusicList.ToList().Shuffle();
		CurrentPlayList.AddRange(_allMusicList);
	}

	public void Setup()
	{
		SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Subscribe(delegate(AudioTag tag)
		{
			CurrentPlayList.Clear();
			IReadOnlyList<GameAudioInfo> source = (IsShuffle ? shuffleList : _allMusicList);
			CurrentPlayList.AddRange(source.Where((GameAudioInfo m) => tag.HasFlagFast(m.Tag) && IsMusicVisible(m)));
		}).AddTo(ref disposable);
	}

	public void Dispose()
	{
		ClearPlayingList();
		disposable.Dispose();
	}

	private void ClearPlayingList()
	{
		foreach (GameAudioInfo allMusic in _allMusicList)
		{
			allMusic.UnloadAudioClip();
		}
		_allMusicList.Clear();
		CurrentPlayList.Clear();
		shuffleList.Clear();
	}

	public bool AddMusicItem(GameAudioInfo music)
	{
		if (music == null)
		{
			return false;
		}
		if (music.PathType == AudioMode.LocalPc)
		{
			return AddLocalMusicItem(music);
		}
		if (_allMusicList.Any((GameAudioInfo m) => m.UUID == music.UUID))
		{
			Debug.LogWarning("UUID:" + music.UUID + " has already been added");
			return false;
		}
		IsPlayListDirtyForLocalImport = SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue.HasFlagFast(music.Tag);
		_allMusicList.Add(music);
		SaveDataManager.Instance.MusicSetting.PlaylistOrder.Add(music.UUID);
		SaveDataManager.Instance.SaveMusicSetting();
		if (SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue.HasFlagFast(music.Tag))
		{
			shuffleList.Add(music);
			CurrentPlayList.Add(music);
		}
		return true;
	}

	public bool AddLocalMusicItem(GameAudioInfo music)
	{
		if (_allMusicList.Count((GameAudioInfo m) => m.PathType == AudioMode.LocalPc) >= 100 || music.PathType != AudioMode.LocalPc || string.IsNullOrEmpty(music.LocalPath))
		{
			return false;
		}
		if (_allMusicList.Any((GameAudioInfo m) => m.LocalPath == music.LocalPath))
		{
			Debug.LogWarning("LocalPath:" + music.LocalPath + " has already been added");
			return false;
		}
		if (_allMusicList.Any((GameAudioInfo m) => m.UUID == music.UUID))
		{
			Debug.LogWarning("UUID:" + music.UUID + " has already been added");
			return false;
		}
		IsPlayListDirtyForLocalImport = true;
		_allMusicList.Add(music);
		SaveDataManager.Instance.MusicSetting.PlaylistOrder.Add(music.UUID);
		SaveDataManager.Instance.SaveMusicSetting();
		if (SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue.HasFlagFast(AudioTag.Local) && IsMusicVisible(music))
		{
			shuffleList.Add(music);
			CurrentPlayList.Add(music);
		}
		onCompleteImportMusic.OnNext(music);
		return true;
	}

	public void RemoveLocalMusicItem(GameAudioInfo music)
	{
		if (music != null && music.PathType == AudioMode.LocalPc)
		{
			IsPlayListDirtyForLocalRemove = true;
			_allMusicList.Remove(music);
			shuffleList.Remove(music);
			CurrentPlayList.Remove(music);
			SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs.Remove(music.UUID);
			SaveDataManager.Instance.LocalMusicSetting.LocalAudioDatas.RemoveAll((LocalAudioData m) => m.UUID == music.UUID);
			SaveDataManager.Instance.MusicSetting.PlaylistOrder.Remove(music.UUID);
			SaveDataManager.Instance.SaveMusicSetting();
			SaveDataManager.Instance.SaveLocalMusicSetting();
			onRemoveLocalMusic.OnNext(music);
			music.Dispose();
		}
	}

	public void OnEndAdjustPlayListForLocalImport()
	{
		IsPlayListDirtyForLocalImport = false;
	}

	public void OnEndAdjustPlayListForLocalRemove()
	{
		IsPlayListDirtyForLocalRemove = false;
	}

	public async UniTask<bool> PlayNextMusic(int nextCount, MusicChangeKind changeKind)
	{
		var (gameAudioInfo, audioClip) = await GetNextGameAudio(nextCount);
		if (gameAudioInfo == null || audioClip == null)
		{
			Debug.LogWarning("GameAudioInfo is null");
			return false;
		}
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.Stop(PlayingMusic.AudioClip);
			PlayingMusic = null;
		}
		if (audioClip.loadState != AudioDataLoadState.Loaded)
		{
			audioClip.LoadAudioData();
		}
		SingletonMonoBehaviour<MusicManager>.Instance.Play(audioClip, 1f, 0f, 1f, IsRepeatOneMusic, allowsDuplicate: true, "", delegate
		{
			SkipCurrentMusic(MusicChangeKind.Auto).Forget();
		});
		PlayingMusic = gameAudioInfo;
		onChangeMusic.OnNext(changeKind);
		onPlayMusic.OnNext(gameAudioInfo);
		return true;
	}

	private async UniTask<(GameAudioInfo, AudioClip)> GetNextGameAudio(int nextCount)
	{
		audioLoadCancellation?.Cancel();
		audioLoadCancellation = new CancellationTokenSource();
		List<string> excludedUUIDs = SaveDataManager.Instance.MusicSetting.ExcludedFromPlaylistUUIDs;
		int sign = ((nextCount >= 0) ? 1 : (-1));
		int currentIndex = ((PlayingMusic != null) ? CurrentPlayList.IndexOf(PlayingMusic) : 0);
		for (int i = 0; i < CurrentPlayList.Count; i++)
		{
			int index = (int)Mathf.Repeat(currentIndex + nextCount + sign * i, CurrentPlayList.Count);
			GameAudioInfo nextMusic = CurrentPlayList[index];
			if (nextMusic != null && !excludedUUIDs.Contains(nextMusic.UUID) && IsMusicVisible(nextMusic))
			{
				AudioClip audioClip = await nextMusic.GetAudioClip(audioLoadCancellation.Token);
				if (!(audioClip == null))
				{
					return (nextMusic, audioClip);
				}
			}
		}
		return (null, null);
	}

	public void PlayArugumentMusic(GameAudioInfo audioInfo, MusicChangeKind changeKind)
	{
		AudioClip audioClip = audioInfo.AudioClip;
		if (audioInfo == null || audioClip == null)
		{
			Debug.LogError($"auidoInfoが正しく設定されていません。 nextMusic⇒{audioInfo}, audioClip⇒{audioClip}");
			return;
		}
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.Stop(PlayingMusic.AudioClip);
			PlayingMusic = null;
		}
		audioClip.LoadAudioData();
		SingletonMonoBehaviour<MusicManager>.Instance.Play(audioClip, 1f, 0f, 1f, IsRepeatOneMusic, allowsDuplicate: true, "", delegate
		{
			PlayNextMusic(1, MusicChangeKind.Auto).Forget();
		});
		PlayingMusic = audioInfo;
		onChangeMusic.OnNext(changeKind);
		onPlayMusic.OnNext(audioInfo);
	}

	public bool PlayMusicInPlaylist(int index)
	{
		if (CurrentPlayList.Count == 0 || !CurrentPlayList.InBounded(index))
		{
			return false;
		}
		GameAudioInfo gameAudioInfo = CurrentPlayList[index];
		AudioClip audioClip = gameAudioInfo.AudioClip;
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.Stop(PlayingMusic.AudioClip);
			PlayingMusic = null;
		}
		if (audioClip.loadState != AudioDataLoadState.Loaded)
		{
			audioClip.LoadAudioData();
		}
		SingletonMonoBehaviour<MusicManager>.Instance.Play(audioClip, 1f, 0f, 1f, IsRepeatOneMusic, allowsDuplicate: true, "", delegate
		{
			PlayNextMusic(1, MusicChangeKind.Auto).Forget();
		});
		PlayingMusic = gameAudioInfo;
		onChangeMusic.OnNext(MusicChangeKind.Manual);
		onPlayMusic.OnNext(gameAudioInfo);
		return true;
	}

	public async UniTask<bool> PlayBackMusic()
	{
		if (PlayingMusic != null)
		{
			AudioPlayer player = SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip);
			if (player == null)
			{
				return false;
			}
			if (player.AudioSource.time >= 5f || CurrentPlayList.Count <= 1)
			{
				PlayArugumentMusic(PlayingMusic, MusicChangeKind.Manual);
				return true;
			}
		}
		if (PlayingMusic == null || !CurrentPlayList.Contains(PlayingMusic))
		{
			return await PlayNextMusic(0, MusicChangeKind.Manual);
		}
		return await PlayNextMusic(-1, MusicChangeKind.Manual);
	}

	public async UniTask<bool> SkipCurrentMusic(MusicChangeKind kind)
	{
		if (PlayingMusic == null || !CurrentPlayList.Contains(PlayingMusic))
		{
			return await PlayNextMusic(0, kind);
		}
		return await PlayNextMusic(1, kind);
	}

	public void Stop()
	{
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip)?.Stop();
		}
		PlayingMusic = null;
	}

	public void Pause()
	{
		if (PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip)?.Pause();
			onPauseMusic.OnNext(PlayingMusic);
		}
	}

	public void UnPause()
	{
		if (PlayingMusic == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip) != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.UnPause(PlayingMusic.AudioClip);
		}
		else
		{
			SingletonMonoBehaviour<MusicManager>.Instance.Play(PlayingMusic.AudioClip, 1f, 0f, 1f, IsRepeatOneMusic, allowsDuplicate: true, "", delegate
			{
				SkipCurrentMusic(MusicChangeKind.Auto).Forget();
			});
		}
		onPlayMusic.OnNext(PlayingMusic);
	}

	public void SetRepeat(bool isrepeat)
	{
		IsRepeatOneMusic = isrepeat;
		if (PlayingMusic != null && !string.IsNullOrEmpty(PlayingMusic.AudioClipName))
		{
			SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip)?.SetLoop(isrepeat);
		}
	}

	public void SetShuffle(bool isShuffle)
	{
		IsShuffle = isShuffle;
		SaveDataManager.Instance.MusicSetting.IsShufflePlayMusic = isShuffle;
		SaveDataManager.Instance.SaveMusicSetting();
		CurrentPlayList.Clear();
		if (isShuffle)
		{
			shuffleList = _allMusicList.ToList().Shuffle();
			if (PlayingMusic != null)
			{
				shuffleList.Move(shuffleList.IndexOf(PlayingMusic), 0);
			}
			onEnableShuffle.OnNext(Unit.Default);
		}
		else
		{
			onDisableShuffle.OnNext(Unit.Default);
		}
		IEnumerable<GameAudioInfo> source = (isShuffle ? shuffleList : _allMusicList);
		CurrentPlayList.AddRange(source.Where((GameAudioInfo m) => SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue.HasFlagFast(m.Tag) && IsMusicVisible(m)));
	}

	public float GetCurrentMusicProgress()
	{
		float result = 0f;
		if (PlayingMusic != null && !string.IsNullOrEmpty(PlayingMusic.AudioClipName))
		{
			AudioPlayer player = SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip);
			if (Mathf.Approximately(player.AudioSource.clip.length, 0f))
			{
				return 0f;
			}
			if (player == null)
			{
				Debug.LogError("audioPlayerが見つからない");
			}
			result = player.AudioSource.time / player.AudioSource.clip.length;
		}
		return result;
	}

	public void SetMusicProgress(float progress)
	{
		if (PlayingMusic != null && !string.IsNullOrEmpty(PlayingMusic.AudioClipName) && progress > 0.001f && progress < 0.999f)
		{
			AudioPlayer player = SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip);
			if (player == null)
			{
				Debug.LogError("audioPlayerが見つからない");
				return;
			}
			float length = player.AudioSource.clip.length;
			player.AudioSource.time = Mathf.Clamp(length * progress, 0f, length);
		}
	}

	public bool RegisterFavoriteMusic(GameAudioInfo gameAudioInfo)
	{
		if (gameAudioInfo.Tag.HasFlagFast(AudioTag.Favorite))
		{
			return false;
		}
		gameAudioInfo.Tag.AddFavorite();
		if (!SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs.Contains(gameAudioInfo.UUID))
		{
			SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs.Add(gameAudioInfo.UUID);
			SaveDataManager.Instance.SaveMusicSetting();
		}
		return true;
	}

	public bool UnregisterFavoriteMusic(GameAudioInfo gameAudioInfo)
	{
		if (!gameAudioInfo.Tag.HasFlagFast(AudioTag.Favorite))
		{
			return false;
		}
		gameAudioInfo.Tag.RemoveFavorite();
		if (!SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue.HasFlagFast(gameAudioInfo.Tag))
		{
			CurrentPlayList.Remove(gameAudioInfo);
			shuffleList.Remove(gameAudioInfo);
		}
		SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs.Remove(gameAudioInfo.UUID);
		SaveDataManager.Instance.SaveMusicSetting();
		return true;
	}

	public bool ExcludeFromPlaylist(GameAudioInfo gameAudioInfo)
	{
		if (IsContainsExcludedFromPlaylist(gameAudioInfo))
		{
			return false;
		}
		SaveDataManager.Instance.MusicSetting.ExcludedFromPlaylistUUIDs.Add(gameAudioInfo.UUID);
		SaveDataManager.Instance.SaveMusicSetting();
		return true;
	}

	public bool IncludeInPlaylist(GameAudioInfo gameAudioInfo)
	{
		if (!IsContainsExcludedFromPlaylist(gameAudioInfo))
		{
			return false;
		}
		SaveDataManager.Instance.MusicSetting.ExcludedFromPlaylistUUIDs.Remove(gameAudioInfo.UUID);
		SaveDataManager.Instance.SaveMusicSetting();
		return true;
	}

	public bool IsContainsExcludedFromPlaylist(GameAudioInfo gameAudioInfo)
	{
		return SaveDataManager.Instance.MusicSetting.ExcludedFromPlaylistUUIDs.Contains(gameAudioInfo.UUID);
	}

	public void SetFavoriteTag()
	{
		favoritePrevTags = SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue;
		SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value = AudioTag.Favorite;
		SaveDataManager.Instance.SaveMusicSetting();
	}

	public void SetMusicListTags()
	{
		SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value = favoritePrevTags;
		SaveDataManager.Instance.SaveMusicSetting();
	}

	public bool HasFlagInCurrentTag(AudioTag checkFlag)
	{
		return SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value.HasFlagFast(checkFlag);
	}

	private bool IsMusicVisible(GameAudioInfo music)
	{
		if (music == null || string.IsNullOrEmpty(music.UUID))
		{
			return true;
		}
		if (music.UUID == "5008cbff-d18d-5339-a12d-b8525d047507")
		{
			DateTime dateTime = new DateTime(2025, 12, 25);
			return DateTime.Now >= dateTime;
		}
		return true;
	}

	public void SwapAfter(GameAudioInfo target, GameAudioInfo origin)
	{
		Core<GameAudioInfo>(CurrentPlayList, target, origin);
		if (IsShuffle)
		{
			Core<GameAudioInfo>(shuffleList, target, origin);
			return;
		}
		if (SaveDataManager.Instance.MusicSetting.CurrentAudioTag.CurrentValue == AudioTag.Favorite)
		{
			Core<string>(SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs, target.UUID, origin?.UUID);
		}
		else
		{
			Core<GameAudioInfo>(_allMusicList, target, origin);
			SaveDataManager.Instance.MusicSetting.PlaylistOrder = _allMusicList.Select((GameAudioInfo x) => x.UUID).ToList();
		}
		SaveDataManager.Instance.SaveMusicSetting();
		static void Core<T>(IList<T> list, T item, T val)
		{
			int num = list.IndexOf(item);
			if (num >= 0)
			{
				int num2 = 0;
				if (val != null)
				{
					num2 = list.IndexOf(val);
				}
				if (num2 >= 0)
				{
					if (val != null && num > num2)
					{
						num2++;
					}
					list.Move(num, num2);
				}
			}
		}
	}
}
